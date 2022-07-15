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

public class ReplyMarkupMapper : ITLObjectMapper<ReplyMarkup, ReplyMarkupDTO>
{
    private readonly ITLObjectFactory _factory;
    private readonly IMapperContext _mapper;
    public ReplyMarkupMapper(ITLObjectFactory factory, IMapperContext mapper)
    {
        _factory = factory;
        _mapper = mapper;
    }
    public ReplyMarkupDTO MapToDTO(ReplyMarkup obj)
    {
        if (obj is ReplyKeyboardMarkupImpl keyboard)
        {
            var rows = new List<KeyboardButtonRowDTO>();
            GenerateKeyboardRows(keyboard, rows);
            
            return new ReplyMarkupDTO(ReplyMarkupType.KeyboardMarkup,
                keyboard.Selective, keyboard.SingleUse, keyboard.Placeholder, keyboard.Resize, rows);
        }
        else
        {
            throw new NotSupportedException("Unknown ReplyMarkup type");
        }
    }

    private void GenerateKeyboardRows(ReplyKeyboardMarkupImpl keyboard, List<KeyboardButtonRowDTO> rows)
    {
        if (keyboard.Rows != null)
        {
            foreach (var r in keyboard.Rows)
            {
                KeyboardButtonRowImpl? rowImpl = r as KeyboardButtonRowImpl;
                var buttons = new List<KeyboardButtonDTO>();
                foreach (var b in rowImpl.Buttons)
                {
                    if (b is KeyboardButtonImpl button)
                    {
                        buttons.Add(new KeyboardButtonDTO(KeyboardButtonType.Button, button.Text,
                            null, false, null, false, null,
                            null, null, false, null, null,
                            null, null));
                    }
                    else if (b is KeyboardButtonSwitchInlineImpl buttonSwitch)
                    {
                        buttons.Add(new KeyboardButtonDTO(KeyboardButtonType.ButtonSwitchInline, buttonSwitch.Text,
                            null, false, null, false, null,
                            null, null, false, null, null,
                            null, null));
                    }
                    else if (b is KeyboardButtonUrlImpl buttonUrl)
                    {
                        buttons.Add(new KeyboardButtonDTO(KeyboardButtonType.ButtonUrl, buttonUrl.Text,
                            buttonUrl.Url, false, null, false, null,
                            null, null, false, null, null,
                            null, null));
                    }
                    else if (b is KeyboardButtonCallbackImpl buttonCallback)
                    {
                        buttons.Add(new KeyboardButtonDTO(KeyboardButtonType.ButtonCallback, buttonCallback.Text,
                            null, buttonCallback.RequiresPassword, buttonCallback.Data, false, null,
                            null, null, false, null, null,
                            null, null));
                    }
                    else if (b is KeyboardButtonRequestPhoneImpl buttonRequestPhoneNumber)
                    {
                        buttons.Add(new KeyboardButtonDTO(KeyboardButtonType.ButtonRequestPhone,
                            buttonRequestPhoneNumber.Text,
                            null, false, null, false, null,
                            null, null, false, null, null,
                            null, null));
                    }
                    else if (b is KeyboardButtonRequestGeoLocationImpl buttonRequestLocation)
                    {
                        buttons.Add(new KeyboardButtonDTO(KeyboardButtonType.ButtonRequestGeoLocation,
                            buttonRequestLocation.Text,
                            null, false, null, false, null,
                            null, null, false, null, null,
                            null, null));
                    }
                    else if (b is KeyboardButtonRequestPollImpl buttonRequestPoll)
                    {
                        buttons.Add(new KeyboardButtonDTO(KeyboardButtonType.ButtonRequestPoll, buttonRequestPoll.Text,
                            null, false, null, false, null,
                            null, null, false, null, buttonRequestPoll.Quiz,
                            null, null));
                    }
                    else if (b is KeyboardButtonGameImpl buttonGame)
                    {
                        buttons.Add(new KeyboardButtonDTO(KeyboardButtonType.ButtonGame, buttonGame.Text,
                            null, false, null, false, null,
                            null, null, false, null, null,
                            null, null));
                    }
                    else if (b is KeyboardButtonBuyImpl buttonBuy)
                    {
                        buttons.Add(new KeyboardButtonDTO(KeyboardButtonType.ButtonBuy, buttonBuy.Text,
                            null, false, null, false, null,
                            null, null, false, null, null,
                            null, null));
                    }
                    else if (b is KeyboardButtonUrlAuthImpl buttonUrlAuth)
                    {
                        buttons.Add(new KeyboardButtonDTO(KeyboardButtonType.ButtonUrlAuth, buttonUrlAuth.Text,
                            buttonUrlAuth.Url, false, null, false, null,
                            buttonUrlAuth.FwdText, buttonUrlAuth.ButtonId, false, null, null,
                            null, null));
                    }
                    else if (b is InputKeyboardButtonUrlAuthImpl inputUrlAuth)
                    {
                        var bot = _mapper.MapToDTO<InputUser, InputUserDTO>(inputUrlAuth.Bot);
                        buttons.Add(new KeyboardButtonDTO(KeyboardButtonType.ButtonUrlAuth, inputUrlAuth.Text,
                            inputUrlAuth.Url, false, null, false, null,
                            inputUrlAuth.FwdText, null, inputUrlAuth.RequestWriteAccess, bot, null,
                            null, null));
                    }
                    else if (b is InputKeyboardButtonUserProfileImpl inputUserProfile)
                    {
                        var userId = _mapper.MapToDTO<InputUser, InputUserDTO>(inputUserProfile.UserId);
                        buttons.Add(new KeyboardButtonDTO(KeyboardButtonType.ButtonUrlAuth, inputUserProfile.Text,
                            null, false, null, false, null,
                            null, null, false, null, null,
                            userId, null));
                    }
                    else if (b is KeyboardButtonUserProfileImpl userProfile)
                    {
                        buttons.Add(new KeyboardButtonDTO(KeyboardButtonType.ButtonUrlAuth, userProfile.Text,
                            null, false, null, false, null,
                            null, null, false, null, null,
                            null, userProfile.UserId));
                    }
                }

                rows.Add(new KeyboardButtonRowDTO(buttons));
            }
        }
    }

    private void GenerateKeyboardRowsFromDTO(ReplyMarkupDTO keyboard, Vector<KeyboardButtonRow> rows)
    {
        if (keyboard.Rows != null)
        {
            foreach (var r in keyboard.Rows)
            {
                var row = _factory.Resolve<KeyboardButtonRowImpl>();
                row.Buttons = _factory.Resolve<Vector<KeyboardButton>>();
                foreach (var b in r.Buttons)
                {
                    if (b.KeyboardButtonType == KeyboardButtonType.Button)
                    {
                        var button = _factory.Resolve<KeyboardButtonImpl>();
                        button.Text = b.Text;
                        row.Buttons.Add(button);
                    }
                    else if (b.KeyboardButtonType == KeyboardButtonType.ButtonUrl)
                    {
                        var button = _factory.Resolve<KeyboardButtonUrlImpl>();
                        button.Text = b.Text;
                        button.Url = b.Url;
                        row.Buttons.Add(button);
                    }
                    else if (b.KeyboardButtonType == KeyboardButtonType.ButtonCallback)
                    {
                        var button = _factory.Resolve<KeyboardButtonCallbackImpl>();
                        button.Text = b.Text;
                        button.Data = b.Data;
                        button.RequiresPassword = b.RequiresPassword;
                        row.Buttons.Add(button);
                    }
                    else if (b.KeyboardButtonType == KeyboardButtonType.ButtonRequestPhone)
                    {
                        var button = _factory.Resolve<KeyboardButtonRequestPhoneImpl>();
                        button.Text = b.Text;
                        row.Buttons.Add(button);
                    }
                    else if (b.KeyboardButtonType == KeyboardButtonType.ButtonRequestGeoLocation)
                    {
                        var button = _factory.Resolve<KeyboardButtonRequestGeoLocationImpl>();
                        button.Text = b.Text;
                        row.Buttons.Add(button);
                    }
                    else if (b.KeyboardButtonType == KeyboardButtonType.ButtonSwitchInline)
                    {
                        var button = _factory.Resolve<KeyboardButtonSwitchInlineImpl>();
                        button.SamePeer = b.SamePeer;
                        button.Text = b.Text;
                        row.Buttons.Add(button);
                    }
                    else if (b.KeyboardButtonType == KeyboardButtonType.ButtonGame)
                    {
                        var button = _factory.Resolve<KeyboardButtonGameImpl>();
                        button.Text = b.Text;
                        row.Buttons.Add(button);
                    }
                    else if (b.KeyboardButtonType == KeyboardButtonType.ButtonBuy)
                    {
                        var button = _factory.Resolve<KeyboardButtonBuyImpl>();
                        button.Text = b.Text;
                        row.Buttons.Add(button);
                    }
                    else if (b.KeyboardButtonType == KeyboardButtonType.ButtonUrlAuth)
                    {
                        var button = _factory.Resolve<KeyboardButtonUrlAuthImpl>();
                        button.Text = b.Text;
                        button.Url = b.Url;
                        button.FwdText = b.FwdText;
                        button.ButtonId = (int)b.ButtonId;
                        row.Buttons.Add(button);
                    }
                    else if (b.KeyboardButtonType == KeyboardButtonType.ButtonRequestPoll)
                    {
                        var button = _factory.Resolve<KeyboardButtonRequestPollImpl>();
                        button.Text = b.Text;
                        button.Quiz = (bool)b.Quiz;
                        row.Buttons.Add(button);
                    }
                    else if (b.KeyboardButtonType == KeyboardButtonType.InputButtonUserProfile)
                    {
                        var button = _factory.Resolve<InputKeyboardButtonUserProfileImpl>();
                        button.Text = b.Text;
                        button.UserId = _mapper.MapToTLObject<InputUser, InputUserDTO>(b.User);
                        row.Buttons.Add(button);
                    }
                    else if (b.KeyboardButtonType == KeyboardButtonType.ButtonUserProfile)
                    {
                        var button = _factory.Resolve<KeyboardButtonUserProfileImpl>();
                        button.Text = b.Text;
                        button.UserId = (long)b.UserId;
                        row.Buttons.Add(button);
                    }
                }
            }
        }
    }

    public ReplyMarkup MapToTLObject(ReplyMarkupDTO obj)
    {
        if (obj.ReplyMarkupType == ReplyMarkupType.KeyboardHide)
        {
            var markup = _factory.Resolve<ReplyKeyboardHideImpl>();
            markup.Selective = obj.Selective;
            return markup;
        }
        else if (obj.ReplyMarkupType == ReplyMarkupType.KeyboardForceReply)
        {
            var markup = _factory.Resolve<ReplyKeyboardForceReplyImpl>();
            markup.SingleUse = obj.SingleUse;
            markup.Selective = obj.Selective;
            if(obj.Placeholder is { Length: > 0 })
            {
                markup.Placeholder = obj.Placeholder;
            }
            return markup;
        }
        else if (obj.ReplyMarkupType == ReplyMarkupType.KeyboardMarkup)
        {
            var markup = _factory.Resolve<ReplyKeyboardMarkupImpl>();
            markup.Selective = obj.Selective;
            markup.SingleUse = obj.SingleUse;
            markup.Placeholder = obj.Placeholder;
            markup.Resize = obj.Resize;
            markup.Rows = _factory.Resolve<Vector<KeyboardButtonRow>>();
            GenerateKeyboardRowsFromDTO(obj, markup.Rows);
        }
        else if (obj.ReplyMarkupType == ReplyMarkupType.InlineMarkup)
        {
            var markup = _factory.Resolve<ReplyInlineMarkupImpl>();
            markup.Rows = _factory.Resolve<Vector<KeyboardButtonRow>>();
            GenerateKeyboardRowsFromDTO(obj, markup.Rows);
        }
        throw new NotSupportedException();
    }
}