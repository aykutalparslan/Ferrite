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

public class PeerNotifySettingsMapper : ITLObjectMapper<PeerNotifySettings, PeerNotifySettingsDTO>
{
    private readonly ITLObjectFactory _factory;
    public PeerNotifySettingsMapper(ITLObjectFactory factory)
    {
        _factory = factory;
    }
    public PeerNotifySettingsDTO MapToDTO(PeerNotifySettings obj)
    {
        throw new NotImplementedException();
    }

    public PeerNotifySettings MapToTLObject(PeerNotifySettingsDTO obj)
    {
        var notifySettings = _factory.Resolve<PeerNotifySettingsImpl>();
        notifySettings.ShowPreviews = obj.ShowPreviews;
        notifySettings.Silent = obj.Silent;
        if (obj.MuteUntil > 0)
        {
            notifySettings.MuteUntil = obj.MuteUntil;
        }

        if (obj.DeviceType == DeviceType.Android)
        {
            if (obj.NotifySoundType == NotifySoundType.Default)
            {
                notifySettings.AndroidSound = _factory.Resolve<NotificationSoundDefaultImpl>();
            }
            else if (obj.NotifySoundType == NotifySoundType.Ringtone)
            {
                var sound = _factory.Resolve<NotificationSoundRingtoneImpl>();
                sound.Id = obj.Id;
                notifySettings.AndroidSound = sound;
            }
            else if (obj.NotifySoundType != NotifySoundType.None)
            {
                notifySettings.AndroidSound = _factory.Resolve<NotificationSoundNoneImpl>();
            }
            else if (obj.NotifySoundType == NotifySoundType.Local)
            {
                var sound = _factory.Resolve<NotificationSoundLocalImpl>();
                sound.Title = obj.Title;
                sound.Data = obj.Data;
                notifySettings.AndroidSound = sound;
            }
        }
        else if (obj.DeviceType == DeviceType.iOS)
        {
            if (obj.NotifySoundType == NotifySoundType.Default)
            {
                notifySettings.iOSSound = _factory.Resolve<NotificationSoundDefaultImpl>();
            }
            else if (obj.NotifySoundType == NotifySoundType.Ringtone)
            {
                var sound = _factory.Resolve<NotificationSoundRingtoneImpl>();
                sound.Id = obj.Id;
                notifySettings.iOSSound = sound;
            }
            else if (obj.NotifySoundType != NotifySoundType.None)
            {
                notifySettings.iOSSound = _factory.Resolve<NotificationSoundNoneImpl>();
            }
            else if (obj.NotifySoundType == NotifySoundType.Local)
            {
                var sound = _factory.Resolve<NotificationSoundLocalImpl>();
                sound.Title = obj.Title;
                sound.Data = obj.Data;
                notifySettings.iOSSound = sound;
            }
        }
        else
        {
            if (obj.NotifySoundType == NotifySoundType.Default)
            {
                notifySettings.OtherSound = _factory.Resolve<NotificationSoundDefaultImpl>();
            }
            else if (obj.NotifySoundType == NotifySoundType.Ringtone)
            {
                var sound = _factory.Resolve<NotificationSoundRingtoneImpl>();
                sound.Id = obj.Id;
                notifySettings.OtherSound = sound;
            }
            else if (obj.NotifySoundType != NotifySoundType.None)
            {
                notifySettings.OtherSound = _factory.Resolve<NotificationSoundNoneImpl>();
            }
            else if (obj.NotifySoundType == NotifySoundType.Local)
            {
                var sound = _factory.Resolve<NotificationSoundLocalImpl>();
                sound.Title = obj.Title;
                sound.Data = obj.Data;
                notifySettings.OtherSound = sound;
            }
        }

        return notifySettings;
    }
}