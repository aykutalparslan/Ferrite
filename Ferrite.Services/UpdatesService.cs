//
//  Project Ferrite is an Implementation of the Telegram Server API
//  Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Affero General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Affero General Public License for more details.
//
//  You should have received a copy of the GNU Affero General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.
//
using System;
using System.Collections.ObjectModel;
using Ferrite.Data;
using Ferrite.Data.Repositories;
using Ferrite.Data.Updates;
using Ferrite.Utils;

namespace Ferrite.Services;

public class UpdatesService : IUpdatesService
{
    private readonly IMTProtoTime _time;
    private readonly ISessionService _sessions;
    private readonly IDistributedPipe _pipe;
    private readonly IPersistentStore _store;
    private readonly IDistributedCache _cache;
    private readonly IUnitOfWork _unitOfWork;
    public UpdatesService(IMTProtoTime time, ISessionService sessions, IDistributedPipe pipe,
        IPersistentStore store, IDistributedCache cache, IUnitOfWork unitOfWork)
    {
        _time = time;
        _sessions = sessions;
        _pipe = pipe;
        _store = store;
        _cache = cache;
        _unitOfWork = unitOfWork;
    }

    public async Task<StateDTO> GetState(long authKeyId)
    {
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        var counter = _cache.GetCounter(auth.UserId + "_pts");
        int pts = (int)await counter.Get();
        return new StateDTO()
        {
            Date = (int)_time.GetUnixTimeInSeconds(),
            Pts = pts,
            Seq = pts//TODO: fix seq
        };
    }

    public async Task<ServiceResult<DifferenceDTO>> GetDifference(long authKeyId, int pts, int date, 
        int qts, int? ptsTotalLimit = null)
    {
        var auth = await _store.GetAuthorizationAsync(authKeyId);
        var counter = _cache.GetCounter(auth.UserId + "_pts");
        int currentPts = (int)await counter.Get();
        var state = new StateDTO()
        {
            Date = (int)_time.GetUnixTimeInSeconds(),
            Pts = currentPts,
            Seq = currentPts//TODO: fix seq
        };
        var messages = await _unitOfWork.MessageRepository.GetMessagesAsync(auth.UserId,
            pts, currentPts, DateTimeOffset.FromUnixTimeSeconds(date));
        List<UserDTO> users = new List<UserDTO>();
        foreach (var message in messages)
        {
            if (message.Out && message.PeerId.PeerType == PeerType.User)
            {
                var user = await _store.GetUserAsync(message.PeerId.PeerId);
                users.Add(user);
            }
        }

        var difference = new DifferenceDTO(messages, Array.Empty<EncryptedMessageDTO>(),
            Array.Empty<UpdateBase>(), Array.Empty<ChatDTO>(),
            users, state);
        return new ServiceResult<DifferenceDTO>(difference, true, ErrorMessages.None);
    }
}

