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

namespace Ferrite.Data;

public record InputMediaDTO
{
    public InputMediaType InputMediaType { get; init; }
    public InputFileDTO? File { get; set; }
    public IEnumerable<InputDocumentDTO>? Stickers { get; set; }
    public int? TtlSeconds { get; set; }
    public InputPhotoDTO? Photo { get; set; }
    public InputGeoPointDTO GeoPoint { get; set; }
    public string? PhoneNumber { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? VCard { get; set; }
    public bool NoSoundVideo { get; set; }
    public bool ForceFile { get; set; }
    public InputFileDTO? Thumb { get; set; }
    public string? MimeType { get; set; }
    public IEnumerable<DocumentAttributeDTO>? Attributes { get; set; }
    public InputDocumentDTO? Document { get; set; }
    public string? Query { get; set; }
    public string? Title { get; set; }
    public string? Address { get; set; }
    public string? Provider { get; set; }
    public string? VenueId { get; set; }
    public string? VenueType { get; set; }
    public string? Url { get; set; }
    public InputGameDTO? Game { get; set; }
    //TODO: add support for invoices
    public bool Stopped { get; set; }
    public int? Heading { get; set; }
    public int? Period { get; set; }
    public int? ProximityNotificationRadius { get; set; }
    public PollDTO? Poll { get; set; }
    public IEnumerable<byte[]>? CorrectAnswers { get; set; }
    public string? Solution { get; set; }
    public IEnumerable<MessageEntityDTO>? SolutionEntities { get; set; }
    public string? Emoticon { get; set; }
    
}