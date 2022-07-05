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

using MessagePack;

namespace Ferrite.Data;

[MessagePackObject(true)] public record PageBlock(PageBlockType PageBlockType,
    RichText? Text, RichText? Author, int? PublishedDate, string? Language, 
    IReadOnlyCollection<PageListItem>? PageListItems, RichText? Caption,
    long? PhotoId, PageCaption? PageCaption, string? Url, long? WebPageId,
    bool Autoplay, bool Loop, long? VideoId, PageBlock? Cover, bool FullWidth,
    bool AllowScrolling, string? Html, long PosterPhotoId, int? W, int? H,
    long? AuthorPhotoId, IReadOnlyCollection<PageBlock>? Blocks, IReadOnlyCollection<PageBlock>? Items,
    Chat? Channel, long? AudioId, bool Bordered, bool Striped, IReadOnlyCollection<PageTableRow>? Rows,
    IReadOnlyCollection<PageListOrderedItem>? PageListOrderedItems, bool Open,
    IReadOnlyCollection<PageRelatedArticle>? Articles, GeoPoint? Geo, int? Zoom);