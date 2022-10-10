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

public class InputGeoPointMapper : ITLObjectMapper<InputGeoPoint, InputGeoPointDTO>
{
    private readonly ITLObjectFactory _factory;
    public InputGeoPointMapper(ITLObjectFactory factory)
    {
        _factory = factory;
    }
    public InputGeoPointDTO MapToDTO(InputGeoPoint obj)
    {
        if(obj is InputGeoPointEmptyImpl)
        {
            return new InputGeoPointDTO(true, null, null, 
                null);
        }
        if (obj is InputGeoPointImpl geo)
        {
            return new InputGeoPointDTO(false, geo.Lat, geo.Long, geo.AccuracyRadius);
        }
        throw new NotSupportedException();
    }

    public InputGeoPoint MapToTLObject(InputGeoPointDTO obj)
    {
        if (obj.Empty) return _factory.Resolve<InputGeoPointEmptyImpl>();
        var geo = _factory.Resolve<InputGeoPointImpl>();
        geo.Lat = (double)obj.Latitude!;
        geo.Long = (double)obj.Longitude!;
        if (obj.AccuracyRadius != null)
        {
            geo.AccuracyRadius = (int)obj.AccuracyRadius;
        }
        return geo;
    }
}