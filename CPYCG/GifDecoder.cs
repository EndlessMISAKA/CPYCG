using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;

namespace CPYCG
{
    public class GifDecoder
    {
        GifFile gifMetadata = new GifFile();

        CCG.LoadPicEventHander LoadPic = null;
        List<string> fileEx = null;
        
        private struct Int32Size
        {
            public Int32Size(int width, int height) : this()
            {
                Width = width;
                Height = height;
            }

            public int Width { get; private set; }
            public int Height { get; private set; }
        }

        private enum FrameDisposalMethod
        {
            None = 0,
            DoNotDispose = 1,
            RestoreBackground = 2,
            RestorePrevious = 3
        }

        private class FrameMetadata
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public int Delay { get; set; }
            public FrameDisposalMethod DisposalMethod { get; set; }
        }

        public GifDecoder(CCG.LoadPicEventHander lpe, List<string> fileExtentions)
        {
            LoadPic = lpe;
            fileEx = fileExtentions;
        }

        public PicData_PlayTime[] GetGifData(string filePath, out bool isNoVariable)
        {
            isNoVariable = true;
            GifBitmapDecoder gbd = new GifBitmapDecoder(new Uri(filePath), BitmapCreateOptions.DelayCreation, BitmapCacheOption.Default);
            if (null == gbd || null == gbd.Metadata || gbd.Frames.Count == 0) return null;
            if (gbd.Frames.Count == 1) return new PicData_PlayTime[1];

            MemoryStream ms = new MemoryStream(File.ReadAllBytes(filePath));
            //gifMetadata = new GifFile(ms, true);
            try
            {
                gifMetadata = new GifFile(ms, true);
            }
            catch
            {
                ms.Close();
                return null;
            }

            var fullSize = GetFullSize(gbd, gifMetadata);
            BitmapSource baseFrame = null;
            ms.Close();
            List<PicData_PlayTime> pdpt = new List<PicData_PlayTime>();
            JpegBitmapEncoder jbe = null;
            int count = gbd.Frames.Count - 1;

            for (int index = 0; index <= count; index++)
            {
                var metadata = GetFrameMetadata(gbd, gifMetadata, index);

                var frame = MakeFrame(fullSize, gbd.Frames[index], metadata, baseFrame);

                jbe = new JpegBitmapEncoder();
                jbe.Frames.Add(BitmapFrame.Create(frame));
                ms = new MemoryStream();
                jbe.Save(ms);
                pdpt.Add(new PicData_PlayTime(ms.ToArray(), metadata.Delay));
                ms.Close();
                fileEx.Add("jpg");
                if (null != LoadPic)
                    LoadPic(pdpt.Last().picData, pdpt.Count);
                if (isNoVariable)
                {
                    if (index > 1)
                    {
                        isNoVariable = (pdpt[index].playTime == pdpt[index - 1].playTime);
                    }
                }
                if (index == count) break;
                switch (metadata.DisposalMethod)
                {
                    case FrameDisposalMethod.None:
                    case FrameDisposalMethod.DoNotDispose:
                        baseFrame = frame;
                        break;
                    case FrameDisposalMethod.RestoreBackground:
                        if (IsFullFrame(metadata, fullSize))
                        {
                            baseFrame = null;
                        }
                        else
                        {
                            baseFrame = ClearArea(frame, metadata);
                        }
                        break;
                    case FrameDisposalMethod.RestorePrevious:
                        // Reuse same base frame
                        break;
                }
            }
            return pdpt.ToArray();
        }

        private BitmapSource ClearArea(BitmapSource frame, FrameMetadata metadata)
        {
            DrawingVisual visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                var fullRect = new Rect(0, 0, frame.PixelWidth, frame.PixelHeight);
                var clearRect = new Rect(metadata.Left, metadata.Top, metadata.Width, metadata.Height);
                var clip = Geometry.Combine(
                    new RectangleGeometry(fullRect),
                    new RectangleGeometry(clearRect),
                    GeometryCombineMode.Exclude,
                    null);
                context.PushClip(clip);
                context.DrawImage(frame, fullRect);
            }

            var bitmap = new RenderTargetBitmap(
                    frame.PixelWidth, frame.PixelHeight,
                    frame.DpiX, frame.DpiY,
                    PixelFormats.Pbgra32);
            bitmap.Render(visual);

            if (bitmap.CanFreeze && !bitmap.IsFrozen)
                bitmap.Freeze();
            return bitmap;
        }

        private BitmapSource MakeFrame(Int32Size fullSize, BitmapSource rawFrame, FrameMetadata metadata, BitmapSource baseFrame)
        {
            if (baseFrame == null && IsFullFrame(metadata, fullSize))
            {
                // No previous image to combine with, and same size as the full image
                // Just return the frame as is
                return rawFrame;
            }

            DrawingVisual visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                if (baseFrame != null)
                {
                    var fullRect = new Rect(0, 0, fullSize.Width, fullSize.Height);
                    context.DrawImage(baseFrame, fullRect);
                }

                var rect = new Rect(metadata.Left, metadata.Top, metadata.Width, metadata.Height);
                context.DrawImage(rawFrame, rect);
            }
            var bitmap = new RenderTargetBitmap(
                fullSize.Width, fullSize.Height,
                96, 96,
                PixelFormats.Pbgra32);
            bitmap.Render(visual);

            if (bitmap.CanFreeze && !bitmap.IsFrozen)
                bitmap.Freeze();
            return bitmap;
        }

        private bool IsFullFrame(FrameMetadata metadata, Int32Size fullSize)
        {
            return metadata.Left == 0
                   && metadata.Top == 0
                   && metadata.Width == fullSize.Width
                   && metadata.Height == fullSize.Height;
        }

        private FrameMetadata GetFrameMetadata(BitmapDecoder decoder, GifFile gifMetadata, int frameIndex)
        {
            if (gifMetadata != null && gifMetadata.Frames.Count > frameIndex)
            {
                return GetFrameMetadata(gifMetadata.Frames[frameIndex]);
            }

            return GetFrameMetadata(decoder.Frames[frameIndex]);
        }

        private FrameMetadata GetFrameMetadata(BitmapFrame frame)
        {
            var metadata = (BitmapMetadata)frame.Metadata;
            var delay = 100;
            var metadataDelay = GetQueryOrDefault(metadata, "/grctlext/Delay", 10);
            if (metadataDelay != 0)
                delay = metadataDelay * 10;
            var disposalMethod = (FrameDisposalMethod)GetQueryOrDefault(metadata, "/grctlext/Disposal", 0);
            var frameMetadata = new FrameMetadata
            {
                Left = GetQueryOrDefault(metadata, "/imgdesc/Left", 0),
                Top = GetQueryOrDefault(metadata, "/imgdesc/Top", 0),
                Width = GetQueryOrDefault(metadata, "/imgdesc/Width", frame.PixelWidth),
                Height = GetQueryOrDefault(metadata, "/imgdesc/Height", frame.PixelHeight),
                Delay = delay,
                DisposalMethod = disposalMethod
            };
            return frameMetadata;
        }

        private FrameMetadata GetFrameMetadata(GifFrame gifMetadata)
        {
            var d = gifMetadata.Descriptor;
            var frameMetadata = new FrameMetadata
            {
                Left = d.Left,
                Top = d.Top,
                Width = d.Width,
                Height = d.Height,
                Delay = 100,
                DisposalMethod = FrameDisposalMethod.None
            };

            var gce = gifMetadata.Extensions.OfType<GifGraphicControlExtension>().FirstOrDefault();
            if (gce != null)
            {
                if (gce.Delay != 0)
                    frameMetadata.Delay = gce.Delay;
                frameMetadata.DisposalMethod = (FrameDisposalMethod)gce.DisposalMethod;
            }
            return frameMetadata;
        }

        private Int32Size GetFullSize(BitmapDecoder decoder, GifFile gifMetadata)
        {
            if (gifMetadata != null)
            {
                var lsd = gifMetadata.Header.LogicalScreenDescriptor;
                return new Int32Size(lsd.Width, lsd.Height);
            }
            int width = GetQueryOrDefault(decoder.Metadata,"/logscrdesc/Width", 0);
            int height = GetQueryOrDefault(decoder.Metadata, "/logscrdesc/Height", 0);
            return new Int32Size(width, height);
        }

        public T GetQueryOrDefault<T>(BitmapMetadata metadata, string query, T defaultValue)
        {
            if (metadata.ContainsQuery(query))
                return (T)Convert.ChangeType(metadata.GetQuery(query), typeof(T));
            return defaultValue;
        }
    }
}
