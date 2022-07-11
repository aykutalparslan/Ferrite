/*
 *   Project Ferrite is an Implementation of the Telegram Server API
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
using Ferrite.TL.ObjectMapper;
using Ferrite.Utils;

namespace Ferrite.TL.currentLayer.photos;
public class UploadProfilePhoto : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IPhotosService _photos;
    private readonly IMapperContext _mapper;
    private bool serialized = false;
    public UploadProfilePhoto(ITLObjectFactory objectFactory, IPhotosService photos, IPersistentStore store,
        IMapperContext mapper)
    {
        factory = objectFactory;
        _photos = photos;
        _mapper = mapper;
    }

    public int Constructor => -1980559511;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            if (_flags[0])
            {
                writer.Write(_file.TLBytes, false);
            }

            if (_flags[1])
            {
                writer.Write(_video.TLBytes, false);
            }

            if (_flags[2])
            {
                writer.Write(BitConverter.GetBytes(_videoStartTs));
            }

            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private Flags _flags;
    public Flags Flags
    {
        get => _flags;
        set
        {
            serialized = false;
            _flags = value;
        }
    }

    private InputFile _file;
    public InputFile File
    {
        get => _file;
        set
        {
            serialized = false;
            _flags[0] = true;
            _file = value;
        }
    }

    private InputFile _video;
    public InputFile Video
    {
        get => _video;
        set
        {
            serialized = false;
            _flags[1] = true;
            _video = value;
        }
    }

    private double _videoStartTs;
    public double VideoStartTs
    {
        get => _videoStartTs;
        set
        {
            serialized = false;
            _flags[2] = true;
            _videoStartTs = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        Data.InputFileDTO? photo = null;
        Data.InputFileDTO? video = null;
        if (_file is InputFileImpl file)
        {
            photo = new Data.InputFileDTO(file.Id, file.Parts, 
                file.Name, file.Md5Checksum, false);
        }
        else if (_file is InputFileImpl bigFile)
        {
            photo = new Data.InputFileDTO(bigFile.Id, bigFile.Parts, 
                bigFile.Name, bigFile.Md5Checksum, true);
        }
        if (_video is InputFileImpl videoFile)
        {
            photo = new Data.InputFileDTO(videoFile.Id, videoFile.Parts, 
                videoFile.Name, videoFile.Md5Checksum, false);
        }
        else if (_video is InputFileImpl bigVideoFile)
        {
            photo = new Data.InputFileDTO(bigVideoFile.Id, bigVideoFile.Parts, 
                bigVideoFile.Name, bigVideoFile.Md5Checksum, true);
        }

        var serviceResult = await _photos.UploadProfilePhoto(ctx.CurrentAuthKeyId,
            photo, video, _videoStartTs);
        var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        if (!serviceResult.Success)
        {
            var err = factory.Resolve<RpcError>();
            err.ErrorCode = serviceResult.ErrorMessage.Code;
            err.ErrorMessage = serviceResult.ErrorMessage.Message;
            result.Result = err;
        }
        else
        {
            var photoResult = factory.Resolve<PhotoImpl>();
            var photoInner = _mapper.MapToTLObject<currentLayer.Photo, PhotoDTO>(serviceResult.Result.PhotoInner);
            photoResult.Photo = photoInner;
            photoResult.Users = factory.Resolve<Vector<User>>();
            foreach (var user in serviceResult.Result.Users)
            {
                var userImpl = _mapper.MapToTLObject<User, UserDTO>(user);
                photoResult.Users.Add(userImpl);
            }
            result.Result = photoResult;
        }

        return result;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        if (_flags[0])
        {
            _file = (InputFile)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[1])
        {
            _video = (InputFile)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[2])
        {
            _videoStartTs = buff.Read<double>();
        }
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}