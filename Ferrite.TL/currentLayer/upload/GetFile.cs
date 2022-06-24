/*
 *   Project Ferrite is an Implementation Telegram Server API
 *   Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *
 *   This program is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU Affero General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU Affero General Public License for more details.
 *
 *   You should have received a copy of the GNU Affero General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Buffers;
using DotNext.Buffers;
using DotNext.IO;
using Ferrite.Data;
using Ferrite.Services;
using Ferrite.TL.mtproto;
using Ferrite.Utils;

namespace Ferrite.TL.currentLayer.upload;
public class GetFile : ITLObject
{
    private readonly ITLObjectFactory _factory;
    private readonly IUploadService _uploadService;
    public GetFile(ITLObjectFactory factory, IUploadService uploadService)
    {
        _factory = factory;
        _uploadService = uploadService;
    }
    public int Constructor => -1319462148;
    public ReadOnlySequence<byte> TLBytes => throw new NotSupportedException();
    private Flags _flags;
    public Flags Flags => _flags;
    public bool Precise => _flags[0];
    public bool CdnSupported => _flags[1];
    private InputFileLocation _location;
    public InputFileLocation Location => _location;
    private int _offset;
    public int Offset => _offset;
    private int _limit;
    public int Limit => _limit;
    public async Task<TLObjectStream> ExecuteAsync(TLExecutionContext ctx)
    {
        if (_location is InputPhotoFileLocationImpl photoLocation)
        {
            var result = await _uploadService.GetPhoto(photoLocation.Id, photoLocation.AccessHash,
                photoLocation.FileReference, photoLocation.ThumbSize, _offset, _limit);
            if (result.Success)
            {
                return new TLObjectStream(result.Result, true, null);
            }

            var err = _factory.Resolve<RpcError>();
            err.ErrorCode = result.ErrorMessage.Code;
            err.ErrorMessage = result.ErrorMessage.Message;
            return new TLObjectStream(null, false, err);
        }
        var err2 = _factory.Resolve<RpcError>();
        err2.ErrorCode = 400;
        err2.ErrorMessage = "LOCATION_INVALID";
        return new TLObjectStream(null, false, err2);
    }

    public void Parse(ref SequenceReader buff)
    {
        _flags = buff.Read<Flags>();
        _location = (InputFileLocation)_factory.Read(buff.ReadInt32(true), ref buff);
        _offset = buff.ReadInt32(true);
        _limit = buff.ReadInt32(true);
    }

    public void WriteTo(Span<byte> buff) => throw new NotSupportedException();
}