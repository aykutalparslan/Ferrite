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

using Ferrite.Data;
using Ferrite.TL.currentLayer;

namespace Ferrite.TL.ObjectMapper;

public class InputPeerNotifySettingsMapper : ITLObjectMapper<InputPeerNotifySettings, PeerNotifySettingsDTO>
{
    private readonly ITLObjectFactory _factory;
    public InputPeerNotifySettingsMapper(ITLObjectFactory factory)
    {
        _factory = factory;
    }

    public PeerNotifySettingsDTO MapToDTO(InputPeerNotifySettings obj)
    {
        var settings = (InputPeerNotifySettingsImpl)obj;
        NotifySoundType soundType = NotifySoundType.Default;
        string? title = null;
        string? data = null;
        long soundId = 0;
        if (settings.Sound is NotificationSoundNoneImpl)
        {
            soundType = NotifySoundType.None;
        }
        else if(settings.Sound is NotificationSoundLocalImpl localSound)
        {
            soundType = NotifySoundType.Local;
            title = localSound.Title;
            data = localSound.Data;
        }
        else if(settings.Sound is NotificationSoundRingtoneImpl ringtoneSound)
        {
            soundType = NotifySoundType.Ringtone;
            soundId = ringtoneSound.Id;
        }
        return new Data.PeerNotifySettingsDTO()
        {
            DeviceType = DeviceType.Android,//TODO: get device type from the db
            Silent = settings.Silent,
            NotifySoundType = soundType,
            MuteUntil = settings.MuteUntil,
            ShowPreviews = settings.ShowPreviews,
            Title = title,
            Data = data,
            Id = soundId,
        };
    }

    public InputPeerNotifySettings MapToTLObject(PeerNotifySettingsDTO obj)
    {
        throw new NotImplementedException();
    }
}