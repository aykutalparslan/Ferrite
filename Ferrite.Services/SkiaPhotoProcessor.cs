// 
// Project Ferrite is an Implementation of the Telegram Server API
// Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
// 

using DotNext;
using SkiaSharp;

namespace Ferrite.Services;

public class SkiaPhotoProcessor : IPhotoProcessor
{
    public byte[]? GenerateThumbnail(ReadOnlySpan<byte> src, int w, ImageFilter type)
    {
        try
        {
            using var bitmap = SKBitmap.Decode(src);
            if (type == ImageFilter.Crop)
            {
                var width = bitmap.Width;
                var height = bitmap.Height;
                var size = Math.Min(width, height);
                var x = (width - size) / 2;
                var y = (height - size) / 2;
                var rect = new SKRectI(x, y, x + size, y + size);
                using var cropped = new SKBitmap(size, size);
                bitmap.ExtractSubset(cropped, rect);
                using var scaled = new SKBitmap(w, w);
                cropped.ScalePixels(scaled, SKFilterQuality.High);
                using var data = scaled.Encode(SKEncodedImageFormat.Jpeg, 100);
                return data.ToArray();
            }
            else
            {
                var size = Math.Max(bitmap.Width, bitmap.Height);
                var box = new SKBitmap(size, size);
                using var canvas = new SKCanvas(box);
                canvas.Clear(SKColors.Black);
                var x = (size - bitmap.Width) / 2;
                var y = (size - bitmap.Height) / 2;
                canvas.DrawBitmap(bitmap, x, y);
                using var scaled = new SKBitmap(w, w);
                box.ScalePixels(scaled, SKFilterQuality.High);
                using var data = scaled.Encode(SKEncodedImageFormat.Jpeg, 100);
                return data.ToArray();
            }
        }
        catch (Exception e)
        {
            return null;
        }
        
    }
    public (int w, int h) GetImageSize(ReadOnlySpan<byte> src)
    {
        try
        {
            using var bitmap = SKBitmap.Decode(src);
            return (bitmap.Width, bitmap.Height);
        }
        catch (Exception e)
        {
            return (0, 0);
        }
    }
}