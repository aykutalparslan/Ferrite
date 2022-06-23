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

namespace Ferrite.Services;

public interface IPhotoProcessor
{
    /// <summary>
    /// Generates a thumbnail of the photo.
    /// </summary>
    /// <param name="src"></param>
    /// <param name="w"></param>
    /// <param name="type"></param>
    /// <returns>Generated thumbnail or null if the operation fails.</returns>
    public byte[]? GenerateThumbnail(ReadOnlySpan<byte> src, int w, ImageFilter type);
    /// <summary>
    /// Gets the dimensions of the image.
    /// </summary>
    /// <param name="src"></param>
    /// <returns>Width and height of the image or (0,0) if the image file is invalid.</returns>
    public (int w, int h) GetImageSize(ReadOnlySpan<byte> src);
}