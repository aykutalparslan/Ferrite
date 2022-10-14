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

namespace Ferrite.TLParser;

public static class Layer146Schema
{
    public const string Schema = @"
boolFalse#bc799737 = Bool;
boolTrue#997275b5 = Bool;
error#c4b9f9bb code:int text:string = Error;
/*
inputPeerEmpty#7f3b18ea = InputPeer;
inputPeerSelf#7da07ec9 = InputPeer;
inputPeerChat#35a95cb9 chat_id:long = InputPeer;
inputPeerUser#dde8a54c user_id:long access_hash:long = InputPeer;
inputPeerChannel#27bcbbfc channel_id:long access_hash:long = InputPeer;
inputPeerUserFromMessage#a87b0a1c peer:InputPeer msg_id:int user_id:long = InputPeer;
inputPeerChannelFromMessage#bd2a0840 peer:InputPeer msg_id:int channel_id:long = InputPeer;

inputUserEmpty#b98886cf = InputUser;
inputUserSelf#f7c1b13f = InputUser;
inputUser#f21158c6 user_id:long access_hash:long = InputUser;
inputUserFromMessage#1da448e2 peer:InputPeer msg_id:int user_id:long = InputUser;

inputPhoneContact#f392b7f4 client_id:long phone:string first_name:string last_name:string = InputContact;

inputFile#f52ff27f id:long parts:int name:string md5_checksum:string = InputFile;
inputFileBig#fa4f0bb5 id:long parts:int name:string = InputFile;

inputMediaEmpty#9664f57f = InputMedia;
inputMediaUploadedPhoto#1e287d04 flags:# file:InputFile stickers:flags.0?Vector<InputDocument> ttl_seconds:flags.1?int = InputMedia;
inputMediaPhoto#b3ba0635 flags:# id:InputPhoto ttl_seconds:flags.0?int = InputMedia;
inputMediaGeoPoint#f9c44144 geo_point:InputGeoPoint = InputMedia;
inputMediaContact#f8ab7dfb phone_number:string first_name:string last_name:string vcard:string = InputMedia;
inputMediaUploadedDocument#5b38c6c1 flags:# nosound_video:flags.3?true force_file:flags.4?true file:InputFile thumb:flags.2?InputFile mime_type:string attributes:Vector<DocumentAttribute> stickers:flags.0?Vector<InputDocument> ttl_seconds:flags.1?int = InputMedia;
inputMediaDocument#33473058 flags:# id:InputDocument ttl_seconds:flags.0?int query:flags.1?string = InputMedia;
inputMediaVenue#c13d1c11 geo_point:InputGeoPoint title:string address:string provider:string venue_id:string venue_type:string = InputMedia;
inputMediaPhotoExternal#e5bbfe1a flags:# url:string ttl_seconds:flags.0?int = InputMedia;
inputMediaDocumentExternal#fb52dc99 flags:# url:string ttl_seconds:flags.0?int = InputMedia;
inputMediaGame#d33f43f3 id:InputGame = InputMedia;
inputMediaInvoice#8eb5a6d5 flags:# title:string description:string photo:flags.0?InputWebDocument invoice:Invoice payload:bytes provider:string provider_data:DataJSON start_param:flags.1?string extended_media:flags.2?InputMedia = InputMedia;
inputMediaGeoLive#971fa843 flags:# stopped:flags.0?true geo_point:InputGeoPoint heading:flags.2?int period:flags.1?int proximity_notification_radius:flags.3?int = InputMedia;
inputMediaPoll#f94e5f1 flags:# poll:Poll correct_answers:flags.0?Vector<bytes> solution:flags.1?string solution_entities:flags.1?Vector<MessageEntity> = InputMedia;
inputMediaDice#e66fbf7b emoticon:string = InputMedia;


inputChatPhotoEmpty#1ca48f57 = InputChatPhoto;
inputChatUploadedPhoto#c642724e flags:# file:flags.0?InputFile video:flags.1?InputFile video_start_ts:flags.2?double = InputChatPhoto;
inputChatPhoto#8953ad37 id:InputPhoto = InputChatPhoto;

inputGeoPointEmpty#e4c123d6 = InputGeoPoint;
inputGeoPoint#48222faf flags:# lat:double long:double accuracy_radius:flags.0?int = InputGeoPoint;

inputPhotoEmpty#1cd7bf0d = InputPhoto;
inputPhoto#3bb3b94a id:long access_hash:long file_reference:bytes = InputPhoto;

inputFileLocation#dfdaabe1 volume_id:long local_id:int secret:long file_reference:bytes = InputFileLocation;
inputEncryptedFileLocation#f5235d55 id:long access_hash:long = InputFileLocation;
inputDocumentFileLocation#bad07584 id:long access_hash:long file_reference:bytes thumb_size:string = InputFileLocation;
inputSecureFileLocation#cbc7ee28 id:long access_hash:long = InputFileLocation;
inputTakeoutFileLocation#29be5899 = InputFileLocation;
inputPhotoFileLocation#40181ffe id:long access_hash:long file_reference:bytes thumb_size:string = InputFileLocation;
inputPhotoLegacyFileLocation#d83466f3 id:long access_hash:long file_reference:bytes volume_id:long local_id:int secret:long = InputFileLocation;
inputPeerPhotoFileLocation#37257e99 flags:# big:flags.0?true peer:InputPeer photo_id:long = InputFileLocation;
inputStickerSetThumb#9d84f3db stickerset:InputStickerSet thumb_version:int = InputFileLocation;
inputGroupCallStream#598a92a flags:# call:InputGroupCall time_ms:long scale:int video_channel:flags.0?int video_quality:flags.0?int = InputFileLocation;

peerUser#59511722 user_id:long = Peer;
peerChat#36c6019a chat_id:long = Peer;
peerChannel#a2a5371e channel_id:long = Peer;

storage.fileUnknown#aa963b05 = storage.FileType;
storage.filePartial#40bc6f52 = storage.FileType;
storage.fileJpeg#7efe0e = storage.FileType;
storage.fileGif#cae1aadf = storage.FileType;
storage.filePng#a4f63c0 = storage.FileType;
storage.filePdf#ae1e508d = storage.FileType;
storage.fileMp3#528a0677 = storage.FileType;
storage.fileMov#4b09ebbc = storage.FileType;
storage.fileMp4#b3cea0e4 = storage.FileType;
storage.fileWebp#1081464c = storage.FileType;

userEmpty#d3bc4b7a id:long = User;
user#5d99adee flags:# self:flags.10?true contact:flags.11?true mutual_contact:flags.12?true deleted:flags.13?true bot:flags.14?true bot_chat_history:flags.15?true bot_nochats:flags.16?true verified:flags.17?true restricted:flags.18?true min:flags.20?true bot_inline_geo:flags.21?true support:flags.23?true scam:flags.24?true apply_min_photo:flags.25?true fake:flags.26?true bot_attach_menu:flags.27?true premium:flags.28?true attach_menu_enabled:flags.29?true id:long access_hash:flags.0?long first_name:flags.1?string last_name:flags.2?string username:flags.3?string phone:flags.4?string photo:flags.5?UserProfilePhoto status:flags.6?UserStatus bot_info_version:flags.14?int restriction_reason:flags.18?Vector<RestrictionReason> bot_inline_placeholder:flags.19?string lang_code:flags.22?string emoji_status:flags.30?EmojiStatus = User;

userProfilePhotoEmpty#4f11bae1 = UserProfilePhoto;
userProfilePhoto#82d1f706 flags:# has_video:flags.0?true photo_id:long stripped_thumb:flags.1?bytes dc_id:int = UserProfilePhoto;

userStatusEmpty#9d05049 = UserStatus;
userStatusOnline#edb93949 expires:int = UserStatus;
userStatusOffline#8c703f was_online:int = UserStatus;
userStatusRecently#e26f42f1 = UserStatus;
userStatusLastWeek#7bf09fc = UserStatus;
userStatusLastMonth#77ebc742 = UserStatus;

chatEmpty#29562865 id:long = Chat;
chat#41cbf256 flags:# creator:flags.0?true left:flags.2?true deactivated:flags.5?true call_active:flags.23?true call_not_empty:flags.24?true noforwards:flags.25?true id:long title:string photo:ChatPhoto participants_count:int date:int version:int migrated_to:flags.6?InputChannel admin_rights:flags.14?ChatAdminRights default_banned_rights:flags.18?ChatBannedRights = Chat;
chatForbidden#6592a1a7 id:long title:string = Chat;
channel#8261ac61 flags:# creator:flags.0?true left:flags.2?true broadcast:flags.5?true verified:flags.7?true megagroup:flags.8?true restricted:flags.9?true signatures:flags.11?true min:flags.12?true scam:flags.19?true has_link:flags.20?true has_geo:flags.21?true slowmode_enabled:flags.22?true call_active:flags.23?true call_not_empty:flags.24?true fake:flags.25?true gigagroup:flags.26?true noforwards:flags.27?true join_to_send:flags.28?true join_request:flags.29?true id:long access_hash:flags.13?long title:string username:flags.6?string photo:ChatPhoto date:int restriction_reason:flags.9?Vector<RestrictionReason> admin_rights:flags.14?ChatAdminRights banned_rights:flags.15?ChatBannedRights default_banned_rights:flags.18?ChatBannedRights participants_count:flags.17?int = Chat;
channelForbidden#17d493d5 flags:# broadcast:flags.5?true megagroup:flags.8?true id:long access_hash:long title:string until_date:flags.16?int = Chat;

chatFull#c9d31138 flags:# can_set_username:flags.7?true has_scheduled:flags.8?true id:long about:string participants:ChatParticipants chat_photo:flags.2?Photo notify_settings:PeerNotifySettings exported_invite:flags.13?ExportedChatInvite bot_info:flags.3?Vector<BotInfo> pinned_msg_id:flags.6?int folder_id:flags.11?int call:flags.12?InputGroupCall ttl_period:flags.14?int groupcall_default_join_as:flags.15?Peer theme_emoticon:flags.16?string requests_pending:flags.17?int recent_requesters:flags.17?Vector<long> available_reactions:flags.18?ChatReactions = ChatFull;
channelFull#f2355507 flags:# can_view_participants:flags.3?true can_set_username:flags.6?true can_set_stickers:flags.7?true hidden_prehistory:flags.10?true can_set_location:flags.16?true has_scheduled:flags.19?true can_view_stats:flags.20?true blocked:flags.22?true flags2:# can_delete_channel:flags2.0?true id:long about:string participants_count:flags.0?int admins_count:flags.1?int kicked_count:flags.2?int banned_count:flags.2?int online_count:flags.13?int read_inbox_max_id:int read_outbox_max_id:int unread_count:int chat_photo:Photo notify_settings:PeerNotifySettings exported_invite:flags.23?ExportedChatInvite bot_info:Vector<BotInfo> migrated_from_chat_id:flags.4?long migrated_from_max_id:flags.4?int pinned_msg_id:flags.5?int stickerset:flags.8?StickerSet available_min_id:flags.9?int folder_id:flags.11?int linked_chat_id:flags.14?long location:flags.15?ChannelLocation slowmode_seconds:flags.17?int slowmode_next_send_date:flags.18?int stats_dc:flags.12?int pts:int call:flags.21?InputGroupCall ttl_period:flags.24?int pending_suggestions:flags.25?Vector<string> groupcall_default_join_as:flags.26?Peer theme_emoticon:flags.27?string requests_pending:flags.28?int recent_requesters:flags.28?Vector<long> default_send_as:flags.29?Peer available_reactions:flags.30?ChatReactions = ChatFull;

chatParticipant#c02d4007 user_id:long inviter_id:long date:int = ChatParticipant;
chatParticipantCreator#e46bcee4 user_id:long = ChatParticipant;
chatParticipantAdmin#a0933f5b user_id:long inviter_id:long date:int = ChatParticipant;

chatParticipantsForbidden#8763d3e1 flags:# chat_id:long self_participant:flags.0?ChatParticipant = ChatParticipants;
chatParticipants#3cbc93f8 chat_id:long participants:Vector<ChatParticipant> version:int = ChatParticipants;

chatPhotoEmpty#37c1011c = ChatPhoto;
chatPhoto#1c6e1c11 flags:# has_video:flags.0?true photo_id:long stripped_thumb:flags.1?bytes dc_id:int = ChatPhoto;

messageEmpty#90a6ca84 flags:# id:int peer_id:flags.0?Peer = Message;
message#38116ee0 flags:# out:flags.1?true mentioned:flags.4?true media_unread:flags.5?true silent:flags.13?true post:flags.14?true from_scheduled:flags.18?true legacy:flags.19?true edit_hide:flags.21?true pinned:flags.24?true noforwards:flags.26?true id:int from_id:flags.8?Peer peer_id:Peer fwd_from:flags.2?MessageFwdHeader via_bot_id:flags.11?long reply_to:flags.3?MessageReplyHeader date:int message:string media:flags.9?MessageMedia reply_markup:flags.6?ReplyMarkup entities:flags.7?Vector<MessageEntity> views:flags.10?int forwards:flags.10?int replies:flags.23?MessageReplies edit_date:flags.15?int post_author:flags.16?string grouped_id:flags.17?long reactions:flags.20?MessageReactions restriction_reason:flags.22?Vector<RestrictionReason> ttl_period:flags.25?int = Message;
messageService#2b085862 flags:# out:flags.1?true mentioned:flags.4?true media_unread:flags.5?true silent:flags.13?true post:flags.14?true legacy:flags.19?true id:int from_id:flags.8?Peer peer_id:Peer reply_to:flags.3?MessageReplyHeader date:int action:MessageAction ttl_period:flags.25?int = Message;

messageMediaEmpty#3ded6320 = MessageMedia;
messageMediaPhoto#695150d7 flags:# photo:flags.0?Photo ttl_seconds:flags.2?int = MessageMedia;
messageMediaGeo#56e0d474 geo:GeoPoint = MessageMedia;
messageMediaContact#70322949 phone_number:string first_name:string last_name:string vcard:string user_id:long = MessageMedia;
messageMediaUnsupported#9f84f49e = MessageMedia;
messageMediaDocument#9cb070d7 flags:# nopremium:flags.3?true document:flags.0?Document ttl_seconds:flags.2?int = MessageMedia;
messageMediaWebPage#a32dd600 webpage:WebPage = MessageMedia;
messageMediaVenue#2ec0533f geo:GeoPoint title:string address:string provider:string venue_id:string venue_type:string = MessageMedia;
messageMediaGame#fdb19008 game:Game = MessageMedia;
messageMediaInvoice#f6a548d3 flags:# shipping_address_requested:flags.1?true test:flags.3?true title:string description:string photo:flags.0?WebDocument receipt_msg_id:flags.2?int currency:string total_amount:long start_param:string extended_media:flags.4?MessageExtendedMedia = MessageMedia;
messageMediaGeoLive#b940c666 flags:# geo:GeoPoint heading:flags.0?int period:int proximity_notification_radius:flags.1?int = MessageMedia;
messageMediaPoll#4bd6e798 poll:Poll results:PollResults = MessageMedia;
messageMediaDice#3f7ee58b value:int emoticon:string = MessageMedia;

messageActionEmpty#b6aef7b0 = MessageAction;
messageActionChatCreate#bd47cbad title:string users:Vector<long> = MessageAction;
messageActionChatEditTitle#b5a1ce5a title:string = MessageAction;
messageActionChatEditPhoto#7fcb13a8 photo:Photo = MessageAction;
messageActionChatDeletePhoto#95e3fbef = MessageAction;
messageActionChatAddUser#15cefd00 users:Vector<long> = MessageAction;
messageActionChatDeleteUser#a43f30cc user_id:long = MessageAction;
messageActionChatJoinedByLink#31224c3 inviter_id:long = MessageAction;
messageActionChannelCreate#95d2ac92 title:string = MessageAction;
messageActionChatMigrateTo#e1037f92 channel_id:long = MessageAction;
messageActionChannelMigrateFrom#ea3948e9 title:string chat_id:long = MessageAction;
messageActionPinMessage#94bd38ed = MessageAction;
messageActionHistoryClear#9fbab604 = MessageAction;
messageActionGameScore#92a72876 game_id:long score:int = MessageAction;
messageActionPaymentSentMe#8f31b327 flags:# recurring_init:flags.2?true recurring_used:flags.3?true currency:string total_amount:long payload:bytes info:flags.0?PaymentRequestedInfo shipping_option_id:flags.1?string charge:PaymentCharge = MessageAction;
messageActionPaymentSent#96163f56 flags:# recurring_init:flags.2?true recurring_used:flags.3?true currency:string total_amount:long invoice_slug:flags.0?string = MessageAction;
messageActionPhoneCall#80e11a7f flags:# video:flags.2?true call_id:long reason:flags.0?PhoneCallDiscardReason duration:flags.1?int = MessageAction;
messageActionScreenshotTaken#4792929b = MessageAction;
messageActionCustomAction#fae69f56 message:string = MessageAction;
messageActionBotAllowed#abe9affe domain:string = MessageAction;
messageActionSecureValuesSentMe#1b287353 values:Vector<SecureValue> credentials:SecureCredentialsEncrypted = MessageAction;
messageActionSecureValuesSent#d95c6154 types:Vector<SecureValueType> = MessageAction;
messageActionContactSignUp#f3f25f76 = MessageAction;
messageActionGeoProximityReached#98e0d697 from_id:Peer to_id:Peer distance:int = MessageAction;
messageActionGroupCall#7a0d7f42 flags:# call:InputGroupCall duration:flags.0?int = MessageAction;
messageActionInviteToGroupCall#502f92f7 call:InputGroupCall users:Vector<long> = MessageAction;
messageActionSetMessagesTTL#aa1afbfd period:int = MessageAction;
messageActionGroupCallScheduled#b3a07661 call:InputGroupCall schedule_date:int = MessageAction;
messageActionSetChatTheme#aa786345 emoticon:string = MessageAction;
messageActionChatJoinedByRequest#ebbca3cb = MessageAction;
messageActionWebViewDataSentMe#47dd8079 text:string data:string = MessageAction;
messageActionWebViewDataSent#b4c38cb5 text:string = MessageAction;
messageActionGiftPremium#aba0f5c6 currency:string amount:long months:int = MessageAction;

dialog#a8edd0f5 flags:# pinned:flags.2?true unread_mark:flags.3?true peer:Peer top_message:int read_inbox_max_id:int read_outbox_max_id:int unread_count:int unread_mentions_count:int unread_reactions_count:int notify_settings:PeerNotifySettings pts:flags.0?int draft:flags.1?DraftMessage folder_id:flags.4?int = Dialog;
dialogFolder#71bd134c flags:# pinned:flags.2?true folder:Folder peer:Peer top_message:int unread_muted_peers_count:int unread_unmuted_peers_count:int unread_muted_messages_count:int unread_unmuted_messages_count:int = Dialog;

photoEmpty#2331b22d id:long = Photo;
photo#fb197a65 flags:# has_stickers:flags.0?true id:long access_hash:long file_reference:bytes date:int sizes:Vector<PhotoSize> video_sizes:flags.1?Vector<VideoSize> dc_id:int = Photo;

photoSizeEmpty#e17e23c type:string = PhotoSize;
photoSize#75c78e60 type:string w:int h:int size:int = PhotoSize;
photoCachedSize#21e1ad6 type:string w:int h:int bytes:bytes = PhotoSize;
photoStrippedSize#e0b0bc2e type:string bytes:bytes = PhotoSize;
photoSizeProgressive#fa3efb95 type:string w:int h:int sizes:Vector<int> = PhotoSize;
photoPathSize#d8214d41 type:string bytes:bytes = PhotoSize;

geoPointEmpty#1117dd5f = GeoPoint;
geoPoint#b2a2f663 flags:# long:double lat:double access_hash:long accuracy_radius:flags.0?int = GeoPoint;

auth.sentCode#5e002502 flags:# type:auth.SentCodeType phone_code_hash:string next_type:flags.1?auth.CodeType timeout:flags.2?int = auth.SentCode;

auth.authorization#33fb7bb8 flags:# setup_password_required:flags.1?true otherwise_relogin_days:flags.1?int tmp_sessions:flags.0?int user:User = auth.Authorization;
auth.authorizationSignUpRequired#44747e9a flags:# terms_of_service:flags.0?help.TermsOfService = auth.Authorization;

auth.exportedAuthorization#b434e2b8 id:long bytes:bytes = auth.ExportedAuthorization;

inputNotifyPeer#b8bc5b0c peer:InputPeer = InputNotifyPeer;
inputNotifyUsers#193b4417 = InputNotifyPeer;
inputNotifyChats#4a95e84e = InputNotifyPeer;
inputNotifyBroadcasts#b1db7c7e = InputNotifyPeer;

inputPeerNotifySettings#df1f002b flags:# show_previews:flags.0?Bool silent:flags.1?Bool mute_until:flags.2?int sound:flags.3?NotificationSound = InputPeerNotifySettings;

peerNotifySettings#a83b0426 flags:# show_previews:flags.0?Bool silent:flags.1?Bool mute_until:flags.2?int ios_sound:flags.3?NotificationSound android_sound:flags.4?NotificationSound other_sound:flags.5?NotificationSound = PeerNotifySettings;

peerSettings#a518110d flags:# report_spam:flags.0?true add_contact:flags.1?true block_contact:flags.2?true share_contact:flags.3?true need_contacts_exception:flags.4?true report_geo:flags.5?true autoarchived:flags.7?true invite_members:flags.8?true request_chat_broadcast:flags.10?true geo_distance:flags.6?int request_chat_title:flags.9?string request_chat_date:flags.9?int = PeerSettings;

wallPaper#a437c3ed id:long flags:# creator:flags.0?true default:flags.1?true pattern:flags.3?true dark:flags.4?true access_hash:long slug:string document:Document settings:flags.2?WallPaperSettings = WallPaper;
wallPaperNoFile#e0804116 id:long flags:# default:flags.1?true dark:flags.4?true settings:flags.2?WallPaperSettings = WallPaper;

inputReportReasonSpam#58dbcab8 = ReportReason;
inputReportReasonViolence#1e22c78d = ReportReason;
inputReportReasonPornography#2e59d922 = ReportReason;
inputReportReasonChildAbuse#adf44ee3 = ReportReason;
inputReportReasonOther#c1e4a2b1 = ReportReason;
inputReportReasonCopyright#9b89f93a = ReportReason;
inputReportReasonGeoIrrelevant#dbd4feed = ReportReason;
inputReportReasonFake#f5ddd6e7 = ReportReason;
inputReportReasonIllegalDrugs#a8eb2be = ReportReason;
inputReportReasonPersonalDetails#9ec7863d = ReportReason;

userFull#c4b1fc3f flags:# blocked:flags.0?true phone_calls_available:flags.4?true phone_calls_private:flags.5?true can_pin_message:flags.7?true has_scheduled:flags.12?true video_calls_available:flags.13?true voice_messages_forbidden:flags.20?true id:long about:flags.1?string settings:PeerSettings profile_photo:flags.2?Photo notify_settings:PeerNotifySettings bot_info:flags.3?BotInfo pinned_msg_id:flags.6?int common_chats_count:int folder_id:flags.11?int ttl_period:flags.14?int theme_emoticon:flags.15?string private_forward_name:flags.16?string bot_group_admin_rights:flags.17?ChatAdminRights bot_broadcast_admin_rights:flags.18?ChatAdminRights premium_gifts:flags.19?Vector<PremiumGiftOption> = UserFull;

contact#145ade0b user_id:long mutual:Bool = Contact;

importedContact#c13e3c50 user_id:long client_id:long = ImportedContact;

contactStatus#16d9703b user_id:long status:UserStatus = ContactStatus;

contacts.contactsNotModified#b74ba9d2 = contacts.Contacts;
contacts.contacts#eae87e42 contacts:Vector<Contact> saved_count:int users:Vector<User> = contacts.Contacts;

contacts.importedContacts#77d01c3b imported:Vector<ImportedContact> popular_invites:Vector<PopularContact> retry_contacts:Vector<long> users:Vector<User> = contacts.ImportedContacts;

contacts.blocked#ade1591 blocked:Vector<PeerBlocked> chats:Vector<Chat> users:Vector<User> = contacts.Blocked;
contacts.blockedSlice#e1664194 count:int blocked:Vector<PeerBlocked> chats:Vector<Chat> users:Vector<User> = contacts.Blocked;

messages.dialogs#15ba6c40 dialogs:Vector<Dialog> messages:Vector<Message> chats:Vector<Chat> users:Vector<User> = messages.Dialogs;
messages.dialogsSlice#71e094f3 count:int dialogs:Vector<Dialog> messages:Vector<Message> chats:Vector<Chat> users:Vector<User> = messages.Dialogs;
messages.dialogsNotModified#f0e3e596 count:int = messages.Dialogs;

messages.messages#8c718e87 messages:Vector<Message> chats:Vector<Chat> users:Vector<User> = messages.Messages;
messages.messagesSlice#3a54685e flags:# inexact:flags.1?true count:int next_rate:flags.0?int offset_id_offset:flags.2?int messages:Vector<Message> chats:Vector<Chat> users:Vector<User> = messages.Messages;
messages.channelMessages#64479808 flags:# inexact:flags.1?true pts:int count:int offset_id_offset:flags.2?int messages:Vector<Message> chats:Vector<Chat> users:Vector<User> = messages.Messages;
messages.messagesNotModified#74535f21 count:int = messages.Messages;

messages.chats#64ff9fd5 chats:Vector<Chat> = messages.Chats;
messages.chatsSlice#9cd81144 count:int chats:Vector<Chat> = messages.Chats;

messages.chatFull#e5d7d19c full_chat:ChatFull chats:Vector<Chat> users:Vector<User> = messages.ChatFull;

messages.affectedHistory#b45c69d1 pts:int pts_count:int offset:int = messages.AffectedHistory;

inputMessagesFilterEmpty#57e2f66c = MessagesFilter;
inputMessagesFilterPhotos#9609a51c = MessagesFilter;
inputMessagesFilterVideo#9fc00e65 = MessagesFilter;
inputMessagesFilterPhotoVideo#56e9f0e4 = MessagesFilter;
inputMessagesFilterDocument#9eddf188 = MessagesFilter;
inputMessagesFilterUrl#7ef0dd87 = MessagesFilter;
inputMessagesFilterGif#ffc86587 = MessagesFilter;
inputMessagesFilterVoice#50f5c392 = MessagesFilter;
inputMessagesFilterMusic#3751b49e = MessagesFilter;
inputMessagesFilterChatPhotos#3a20ecb8 = MessagesFilter;
inputMessagesFilterPhoneCalls#80c99768 flags:# missed:flags.0?true = MessagesFilter;
inputMessagesFilterRoundVoice#7a7c17a4 = MessagesFilter;
inputMessagesFilterRoundVideo#b549da53 = MessagesFilter;
inputMessagesFilterMyMentions#c1f8e69a = MessagesFilter;
inputMessagesFilterGeo#e7026d0d = MessagesFilter;
inputMessagesFilterContacts#e062db83 = MessagesFilter;
inputMessagesFilterPinned#1bb00451 = MessagesFilter;

updateNewMessage#1f2b0afd message:Message pts:int pts_count:int = Update;
updateMessageID#4e90bfd6 id:int random_id:long = Update;
updateDeleteMessages#a20db0e5 messages:Vector<int> pts:int pts_count:int = Update;
updateUserTyping#c01e857f user_id:long action:SendMessageAction = Update;
updateChatUserTyping#83487af0 chat_id:long from_id:Peer action:SendMessageAction = Update;
updateChatParticipants#7761198 participants:ChatParticipants = Update;
updateUserStatus#e5bdf8de user_id:long status:UserStatus = Update;
updateUserName#c3f202e0 user_id:long first_name:string last_name:string username:string = Update;
updateUserPhoto#f227868c user_id:long date:int photo:UserProfilePhoto previous:Bool = Update;
updateNewEncryptedMessage#12bcbd9a message:EncryptedMessage qts:int = Update;
updateEncryptedChatTyping#1710f156 chat_id:int = Update;
updateEncryption#b4a2e88d chat:EncryptedChat date:int = Update;
updateEncryptedMessagesRead#38fe25b7 chat_id:int max_date:int date:int = Update;
updateChatParticipantAdd#3dda5451 chat_id:long user_id:long inviter_id:long date:int version:int = Update;
updateChatParticipantDelete#e32f3d77 chat_id:long user_id:long version:int = Update;
updateDcOptions#8e5e9873 dc_options:Vector<DcOption> = Update;
updateNotifySettings#bec268ef peer:NotifyPeer notify_settings:PeerNotifySettings = Update;
updateServiceNotification#ebe46819 flags:# popup:flags.0?true inbox_date:flags.1?int type:string message:string media:MessageMedia entities:Vector<MessageEntity> = Update;
updatePrivacy#ee3b272a key:PrivacyKey rules:Vector<PrivacyRule> = Update;
updateUserPhone#5492a13 user_id:long phone:string = Update;
updateReadHistoryInbox#9c974fdf flags:# folder_id:flags.0?int peer:Peer max_id:int still_unread_count:int pts:int pts_count:int = Update;
updateReadHistoryOutbox#2f2f21bf peer:Peer max_id:int pts:int pts_count:int = Update;
updateWebPage#7f891213 webpage:WebPage pts:int pts_count:int = Update;
updateReadMessagesContents#68c13933 messages:Vector<int> pts:int pts_count:int = Update;
updateChannelTooLong#108d941f flags:# channel_id:long pts:flags.0?int = Update;
updateChannel#635b4c09 channel_id:long = Update;
updateNewChannelMessage#62ba04d9 message:Message pts:int pts_count:int = Update;
updateReadChannelInbox#922e6e10 flags:# folder_id:flags.0?int channel_id:long max_id:int still_unread_count:int pts:int = Update;
updateDeleteChannelMessages#c32d5b12 channel_id:long messages:Vector<int> pts:int pts_count:int = Update;
updateChannelMessageViews#f226ac08 channel_id:long id:int views:int = Update;
updateChatParticipantAdmin#d7ca61a2 chat_id:long user_id:long is_admin:Bool version:int = Update;
updateNewStickerSet#688a30aa stickerset:messages.StickerSet = Update;
updateStickerSetsOrder#bb2d201 flags:# masks:flags.0?true emojis:flags.1?true order:Vector<long> = Update;
updateStickerSets#31c24808 flags:# masks:flags.0?true emojis:flags.1?true = Update;
updateSavedGifs#9375341e = Update;
updateBotInlineQuery#496f379c flags:# query_id:long user_id:long query:string geo:flags.0?GeoPoint peer_type:flags.1?InlineQueryPeerType offset:string = Update;
updateBotInlineSend#12f12a07 flags:# user_id:long query:string geo:flags.0?GeoPoint id:string msg_id:flags.1?InputBotInlineMessageID = Update;
updateEditChannelMessage#1b3f4df7 message:Message pts:int pts_count:int = Update;
updateBotCallbackQuery#b9cfc48d flags:# query_id:long user_id:long peer:Peer msg_id:int chat_instance:long data:flags.0?bytes game_short_name:flags.1?string = Update;
updateEditMessage#e40370a3 message:Message pts:int pts_count:int = Update;
updateInlineBotCallbackQuery#691e9052 flags:# query_id:long user_id:long msg_id:InputBotInlineMessageID chat_instance:long data:flags.0?bytes game_short_name:flags.1?string = Update;
updateReadChannelOutbox#b75f99a9 channel_id:long max_id:int = Update;
updateDraftMessage#ee2bb969 peer:Peer draft:DraftMessage = Update;
updateReadFeaturedStickers#571d2742 = Update;
updateRecentStickers#9a422c20 = Update;
updateConfig#a229dd06 = Update;
updatePtsChanged#3354678f = Update;
updateChannelWebPage#2f2ba99f channel_id:long webpage:WebPage pts:int pts_count:int = Update;
updateDialogPinned#6e6fe51c flags:# pinned:flags.0?true folder_id:flags.1?int peer:DialogPeer = Update;
updatePinnedDialogs#fa0f3ca2 flags:# folder_id:flags.1?int order:flags.0?Vector<DialogPeer> = Update;
updateBotWebhookJSON#8317c0c3 data:DataJSON = Update;
updateBotWebhookJSONQuery#9b9240a6 query_id:long data:DataJSON timeout:int = Update;
updateBotShippingQuery#b5aefd7d query_id:long user_id:long payload:bytes shipping_address:PostAddress = Update;
updateBotPrecheckoutQuery#8caa9a96 flags:# query_id:long user_id:long payload:bytes info:flags.0?PaymentRequestedInfo shipping_option_id:flags.1?string currency:string total_amount:long = Update;
updatePhoneCall#ab0f6b1e phone_call:PhoneCall = Update;
updateLangPackTooLong#46560264 lang_code:string = Update;
updateLangPack#56022f4d difference:LangPackDifference = Update;
updateFavedStickers#e511996d = Update;
updateChannelReadMessagesContents#44bdd535 channel_id:long messages:Vector<int> = Update;
updateContactsReset#7084a7be = Update;
updateChannelAvailableMessages#b23fc698 channel_id:long available_min_id:int = Update;
updateDialogUnreadMark#e16459c3 flags:# unread:flags.0?true peer:DialogPeer = Update;
updateMessagePoll#aca1657b flags:# poll_id:long poll:flags.0?Poll results:PollResults = Update;
updateChatDefaultBannedRights#54c01850 peer:Peer default_banned_rights:ChatBannedRights version:int = Update;
updateFolderPeers#19360dc0 folder_peers:Vector<FolderPeer> pts:int pts_count:int = Update;
updatePeerSettings#6a7e7366 peer:Peer settings:PeerSettings = Update;
updatePeerLocated#b4afcfb0 peers:Vector<PeerLocated> = Update;
updateNewScheduledMessage#39a51dfb message:Message = Update;
updateDeleteScheduledMessages#90866cee peer:Peer messages:Vector<int> = Update;
updateTheme#8216fba3 theme:Theme = Update;
updateGeoLiveViewed#871fb939 peer:Peer msg_id:int = Update;
updateLoginToken#564fe691 = Update;
updateMessagePollVote#106395c9 poll_id:long user_id:long options:Vector<bytes> qts:int = Update;
updateDialogFilter#26ffde7d flags:# id:int filter:flags.0?DialogFilter = Update;
updateDialogFilterOrder#a5d72105 order:Vector<int> = Update;
updateDialogFilters#3504914f = Update;
updatePhoneCallSignalingData#2661bf09 phone_call_id:long data:bytes = Update;
updateChannelMessageForwards#d29a27f4 channel_id:long id:int forwards:int = Update;
updateReadChannelDiscussionInbox#d6b19546 flags:# channel_id:long top_msg_id:int read_max_id:int broadcast_id:flags.0?long broadcast_post:flags.0?int = Update;
updateReadChannelDiscussionOutbox#695c9e7c channel_id:long top_msg_id:int read_max_id:int = Update;
updatePeerBlocked#246a4b22 peer_id:Peer blocked:Bool = Update;
updateChannelUserTyping#8c88c923 flags:# channel_id:long top_msg_id:flags.0?int from_id:Peer action:SendMessageAction = Update;
updatePinnedMessages#ed85eab5 flags:# pinned:flags.0?true peer:Peer messages:Vector<int> pts:int pts_count:int = Update;
updatePinnedChannelMessages#5bb98608 flags:# pinned:flags.0?true channel_id:long messages:Vector<int> pts:int pts_count:int = Update;
updateChat#f89a6a4e chat_id:long = Update;
updateGroupCallParticipants#f2ebdb4e call:InputGroupCall participants:Vector<GroupCallParticipant> version:int = Update;
updateGroupCall#14b24500 chat_id:long call:GroupCall = Update;
updatePeerHistoryTTL#bb9bb9a5 flags:# peer:Peer ttl_period:flags.0?int = Update;
updateChatParticipant#d087663a flags:# chat_id:long date:int actor_id:long user_id:long prev_participant:flags.0?ChatParticipant new_participant:flags.1?ChatParticipant invite:flags.2?ExportedChatInvite qts:int = Update;
updateChannelParticipant#985d3abb flags:# channel_id:long date:int actor_id:long user_id:long prev_participant:flags.0?ChannelParticipant new_participant:flags.1?ChannelParticipant invite:flags.2?ExportedChatInvite qts:int = Update;
updateBotStopped#c4870a49 user_id:long date:int stopped:Bool qts:int = Update;
updateGroupCallConnection#b783982 flags:# presentation:flags.0?true params:DataJSON = Update;
updateBotCommands#4d712f2e peer:Peer bot_id:long commands:Vector<BotCommand> = Update;
updatePendingJoinRequests#7063c3db peer:Peer requests_pending:int recent_requesters:Vector<long> = Update;
updateBotChatInviteRequester#11dfa986 peer:Peer date:int user_id:long about:string invite:ExportedChatInvite qts:int = Update;
updateMessageReactions#154798c3 peer:Peer msg_id:int reactions:MessageReactions = Update;
updateAttachMenuBots#17b7a20b = Update;
updateWebViewResultSent#1592b79d query_id:long = Update;
updateBotMenuButton#14b85813 bot_id:long button:BotMenuButton = Update;
updateSavedRingtones#74d8be99 = Update;
updateTranscribedAudio#84cd5a flags:# pending:flags.0?true peer:Peer msg_id:int transcription_id:long text:string = Update;
updateReadFeaturedEmojiStickers#fb4c496c = Update;
updateUserEmojiStatus#28373599 user_id:long emoji_status:EmojiStatus = Update;
updateRecentEmojiStatuses#30f443db = Update;
updateRecentReactions#6f7863f4 = Update;
updateMoveStickerSetToTop#86fccf85 flags:# masks:flags.0?true emojis:flags.1?true stickerset:long = Update;
updateMessageExtendedMedia#5a73a98c peer:Peer msg_id:int extended_media:MessageExtendedMedia = Update;

updates.state#a56c2a3e pts:int qts:int date:int seq:int unread_count:int = updates.State;

updates.differenceEmpty#5d75a138 date:int seq:int = updates.Difference;
updates.difference#f49ca0 new_messages:Vector<Message> new_encrypted_messages:Vector<EncryptedMessage> other_updates:Vector<Update> chats:Vector<Chat> users:Vector<User> state:updates.State = updates.Difference;
updates.differenceSlice#a8fb1981 new_messages:Vector<Message> new_encrypted_messages:Vector<EncryptedMessage> other_updates:Vector<Update> chats:Vector<Chat> users:Vector<User> intermediate_state:updates.State = updates.Difference;
updates.differenceTooLong#4afe8f6d pts:int = updates.Difference;

updatesTooLong#e317af7e = Updates;
updateShortMessage#313bc7f8 flags:# out:flags.1?true mentioned:flags.4?true media_unread:flags.5?true silent:flags.13?true id:int user_id:long message:string pts:int pts_count:int date:int fwd_from:flags.2?MessageFwdHeader via_bot_id:flags.11?long reply_to:flags.3?MessageReplyHeader entities:flags.7?Vector<MessageEntity> ttl_period:flags.25?int = Updates;
updateShortChatMessage#4d6deea5 flags:# out:flags.1?true mentioned:flags.4?true media_unread:flags.5?true silent:flags.13?true id:int from_id:long chat_id:long message:string pts:int pts_count:int date:int fwd_from:flags.2?MessageFwdHeader via_bot_id:flags.11?long reply_to:flags.3?MessageReplyHeader entities:flags.7?Vector<MessageEntity> ttl_period:flags.25?int = Updates;
updateShort#78d4dec1 update:Update date:int = Updates;
updatesCombined#725b04c3 updates:Vector<Update> users:Vector<User> chats:Vector<Chat> date:int seq_start:int seq:int = Updates;
updates#74ae4240 updates:Vector<Update> users:Vector<User> chats:Vector<Chat> date:int seq:int = Updates;
updateShortSentMessage#9015e101 flags:# out:flags.1?true id:int pts:int pts_count:int date:int media:flags.9?MessageMedia entities:flags.7?Vector<MessageEntity> ttl_period:flags.25?int = Updates;

photos.photos#8dca6aa5 photos:Vector<Photo> users:Vector<User> = photos.Photos;
photos.photosSlice#15051f54 count:int photos:Vector<Photo> users:Vector<User> = photos.Photos;

photos.photo#20212ca8 photo:Photo users:Vector<User> = photos.Photo;

upload.file#96a18d5 type:storage.FileType mtime:int bytes:bytes = upload.File;
upload.fileCdnRedirect#f18cda44 dc_id:int file_token:bytes encryption_key:bytes encryption_iv:bytes file_hashes:Vector<FileHash> = upload.File;

dcOption#18b7a10d flags:# ipv6:flags.0?true media_only:flags.1?true tcpo_only:flags.2?true cdn:flags.3?true static:flags.4?true this_port_only:flags.5?true id:int ip_address:string port:int secret:flags.10?bytes = DcOption;

config#232566ac flags:# phonecalls_enabled:flags.1?true default_p2p_contacts:flags.3?true preload_featured_stickers:flags.4?true ignore_phone_entities:flags.5?true revoke_pm_inbox:flags.6?true blocked_mode:flags.8?true pfs_enabled:flags.13?true force_try_ipv6:flags.14?true date:int expires:int test_mode:Bool this_dc:int dc_options:Vector<DcOption> dc_txt_domain_name:string chat_size_max:int megagroup_size_max:int forwarded_count_max:int online_update_period_ms:int offline_blur_timeout_ms:int offline_idle_timeout_ms:int online_cloud_timeout_ms:int notify_cloud_delay_ms:int notify_default_delay_ms:int push_chat_period_ms:int push_chat_limit:int saved_gifs_limit:int edit_time_limit:int revoke_time_limit:int revoke_pm_time_limit:int rating_e_decay:int stickers_recent_limit:int stickers_faved_limit:int channels_read_media_period:int tmp_sessions:flags.0?int pinned_dialogs_count_max:int pinned_infolder_count_max:int call_receive_timeout_ms:int call_ring_timeout_ms:int call_connect_timeout_ms:int call_packet_timeout_ms:int me_url_prefix:string autoupdate_url_prefix:flags.7?string gif_search_username:flags.9?string venue_search_username:flags.10?string img_search_username:flags.11?string static_maps_provider:flags.12?string caption_length_max:int message_length_max:int webfile_dc_id:int suggested_lang_code:flags.2?string lang_pack_version:flags.2?int base_lang_pack_version:flags.2?int reactions_default:flags.15?Reaction = Config;

nearestDc#8e1a1775 country:string this_dc:int nearest_dc:int = NearestDc;

help.appUpdate#ccbbce30 flags:# can_not_skip:flags.0?true id:int version:string text:string entities:Vector<MessageEntity> document:flags.1?Document url:flags.2?string sticker:flags.3?Document = help.AppUpdate;
help.noAppUpdate#c45a6536 = help.AppUpdate;

help.inviteText#18cb9f78 message:string = help.InviteText;

encryptedChatEmpty#ab7ec0a0 id:int = EncryptedChat;
encryptedChatWaiting#66b25953 id:int access_hash:long date:int admin_id:long participant_id:long = EncryptedChat;
encryptedChatRequested#48f1d94c flags:# folder_id:flags.0?int id:int access_hash:long date:int admin_id:long participant_id:long g_a:bytes = EncryptedChat;
encryptedChat#61f0d4c7 id:int access_hash:long date:int admin_id:long participant_id:long g_a_or_b:bytes key_fingerprint:long = EncryptedChat;
encryptedChatDiscarded#1e1c7c45 flags:# history_deleted:flags.0?true id:int = EncryptedChat;

inputEncryptedChat#f141b5e1 chat_id:int access_hash:long = InputEncryptedChat;

encryptedFileEmpty#c21f497e = EncryptedFile;
encryptedFile#a8008cd8 id:long access_hash:long size:long dc_id:int key_fingerprint:int = EncryptedFile;

inputEncryptedFileEmpty#1837c364 = InputEncryptedFile;
inputEncryptedFileUploaded#64bd0306 id:long parts:int md5_checksum:string key_fingerprint:int = InputEncryptedFile;
inputEncryptedFile#5a17b5e5 id:long access_hash:long = InputEncryptedFile;
inputEncryptedFileBigUploaded#2dc173c8 id:long parts:int key_fingerprint:int = InputEncryptedFile;

encryptedMessage#ed18c118 random_id:long chat_id:int date:int bytes:bytes file:EncryptedFile = EncryptedMessage;
encryptedMessageService#23734b06 random_id:long chat_id:int date:int bytes:bytes = EncryptedMessage;

messages.dhConfigNotModified#c0e24635 random:bytes = messages.DhConfig;
messages.dhConfig#2c221edd g:int p:bytes version:int random:bytes = messages.DhConfig;

messages.sentEncryptedMessage#560f8935 date:int = messages.SentEncryptedMessage;
messages.sentEncryptedFile#9493ff32 date:int file:EncryptedFile = messages.SentEncryptedMessage;

inputDocumentEmpty#72f0eaae = InputDocument;
inputDocument#1abfb575 id:long access_hash:long file_reference:bytes = InputDocument;

documentEmpty#36f8c871 id:long = Document;
document#8fd4c4d8 flags:# id:long access_hash:long file_reference:bytes date:int mime_type:string size:long thumbs:flags.0?Vector<PhotoSize> video_thumbs:flags.1?Vector<VideoSize> dc_id:int attributes:Vector<DocumentAttribute> = Document;

help.support#17c6b5f6 phone_number:string user:User = help.Support;

notifyPeer#9fd40bd8 peer:Peer = NotifyPeer;
notifyUsers#b4c83b4c = NotifyPeer;
notifyChats#c007cec3 = NotifyPeer;
notifyBroadcasts#d612e8ef = NotifyPeer;

sendMessageTypingAction#16bf744e = SendMessageAction;
sendMessageCancelAction#fd5ec8f5 = SendMessageAction;
sendMessageRecordVideoAction#a187d66f = SendMessageAction;
sendMessageUploadVideoAction#e9763aec progress:int = SendMessageAction;
sendMessageRecordAudioAction#d52f73f7 = SendMessageAction;
sendMessageUploadAudioAction#f351d7ab progress:int = SendMessageAction;
sendMessageUploadPhotoAction#d1d34a26 progress:int = SendMessageAction;
sendMessageUploadDocumentAction#aa0cd9e4 progress:int = SendMessageAction;
sendMessageGeoLocationAction#176f8ba1 = SendMessageAction;
sendMessageChooseContactAction#628cbc6f = SendMessageAction;
sendMessageGamePlayAction#dd6a8f48 = SendMessageAction;
sendMessageRecordRoundAction#88f27fbc = SendMessageAction;
sendMessageUploadRoundAction#243e1c66 progress:int = SendMessageAction;
speakingInGroupCallAction#d92c2285 = SendMessageAction;
sendMessageHistoryImportAction#dbda9246 progress:int = SendMessageAction;
sendMessageChooseStickerAction#b05ac6b1 = SendMessageAction;
sendMessageEmojiInteraction#25972bcb emoticon:string msg_id:int interaction:DataJSON = SendMessageAction;
sendMessageEmojiInteractionSeen#b665902e emoticon:string = SendMessageAction;

contacts.found#b3134d9d my_results:Vector<Peer> results:Vector<Peer> chats:Vector<Chat> users:Vector<User> = contacts.Found;

inputPrivacyKeyStatusTimestamp#4f96cb18 = InputPrivacyKey;
inputPrivacyKeyChatInvite#bdfb0426 = InputPrivacyKey;
inputPrivacyKeyPhoneCall#fabadc5f = InputPrivacyKey;
inputPrivacyKeyPhoneP2P#db9e70d2 = InputPrivacyKey;
inputPrivacyKeyForwards#a4dd4c08 = InputPrivacyKey;
inputPrivacyKeyProfilePhoto#5719bacc = InputPrivacyKey;
inputPrivacyKeyPhoneNumber#352dafa = InputPrivacyKey;
inputPrivacyKeyAddedByPhone#d1219bdd = InputPrivacyKey;
inputPrivacyKeyVoiceMessages#aee69d68 = InputPrivacyKey;

privacyKeyStatusTimestamp#bc2eab30 = PrivacyKey;
privacyKeyChatInvite#500e6dfa = PrivacyKey;
privacyKeyPhoneCall#3d662b7b = PrivacyKey;
privacyKeyPhoneP2P#39491cc8 = PrivacyKey;
privacyKeyForwards#69ec56a3 = PrivacyKey;
privacyKeyProfilePhoto#96151fed = PrivacyKey;
privacyKeyPhoneNumber#d19ae46d = PrivacyKey;
privacyKeyAddedByPhone#42ffd42b = PrivacyKey;
privacyKeyVoiceMessages#697f414 = PrivacyKey;

inputPrivacyValueAllowContacts#d09e07b = InputPrivacyRule;
inputPrivacyValueAllowAll#184b35ce = InputPrivacyRule;
inputPrivacyValueAllowUsers#131cc67f users:Vector<InputUser> = InputPrivacyRule;
inputPrivacyValueDisallowContacts#ba52007 = InputPrivacyRule;
inputPrivacyValueDisallowAll#d66b66c9 = InputPrivacyRule;
inputPrivacyValueDisallowUsers#90110467 users:Vector<InputUser> = InputPrivacyRule;
inputPrivacyValueAllowChatParticipants#840649cf chats:Vector<long> = InputPrivacyRule;
inputPrivacyValueDisallowChatParticipants#e94f0f86 chats:Vector<long> = InputPrivacyRule;

privacyValueAllowContacts#fffe1bac = PrivacyRule;
privacyValueAllowAll#65427b82 = PrivacyRule;
privacyValueAllowUsers#b8905fb2 users:Vector<long> = PrivacyRule;
privacyValueDisallowContacts#f888fa1a = PrivacyRule;
privacyValueDisallowAll#8b73e763 = PrivacyRule;
privacyValueDisallowUsers#e4621141 users:Vector<long> = PrivacyRule;
privacyValueAllowChatParticipants#6b134e8e chats:Vector<long> = PrivacyRule;
privacyValueDisallowChatParticipants#41c87565 chats:Vector<long> = PrivacyRule;

account.privacyRules#50a04e45 rules:Vector<PrivacyRule> chats:Vector<Chat> users:Vector<User> = account.PrivacyRules;

accountDaysTTL#b8d0afdf days:int = AccountDaysTTL;

documentAttributeImageSize#6c37c15c w:int h:int = DocumentAttribute;
documentAttributeAnimated#11b58939 = DocumentAttribute;
documentAttributeSticker#6319d612 flags:# mask:flags.1?true alt:string stickerset:InputStickerSet mask_coords:flags.0?MaskCoords = DocumentAttribute;
documentAttributeVideo#ef02ce6 flags:# round_message:flags.0?true supports_streaming:flags.1?true duration:int w:int h:int = DocumentAttribute;
documentAttributeAudio#9852f9c6 flags:# voice:flags.10?true duration:int title:flags.0?string performer:flags.1?string waveform:flags.2?bytes = DocumentAttribute;
documentAttributeFilename#15590068 file_name:string = DocumentAttribute;
documentAttributeHasStickers#9801d2f7 = DocumentAttribute;
documentAttributeCustomEmoji#fd149899 flags:# free:flags.0?true alt:string stickerset:InputStickerSet = DocumentAttribute;

messages.stickersNotModified#f1749a22 = messages.Stickers;
messages.stickers#30a6ec7e hash:long stickers:Vector<Document> = messages.Stickers;

stickerPack#12b299d4 emoticon:string documents:Vector<long> = StickerPack;

messages.allStickersNotModified#e86602c3 = messages.AllStickers;
messages.allStickers#cdbbcebb hash:long sets:Vector<StickerSet> = messages.AllStickers;

messages.affectedMessages#84d19185 pts:int pts_count:int = messages.AffectedMessages;

webPageEmpty#eb1477e8 id:long = WebPage;
webPagePending#c586da1c id:long date:int = WebPage;
webPage#e89c45b2 flags:# id:long url:string display_url:string hash:int type:flags.0?string site_name:flags.1?string title:flags.2?string description:flags.3?string photo:flags.4?Photo embed_url:flags.5?string embed_type:flags.5?string embed_width:flags.6?int embed_height:flags.6?int duration:flags.7?int author:flags.8?string document:flags.9?Document cached_page:flags.10?Page attributes:flags.12?Vector<WebPageAttribute> = WebPage;
webPageNotModified#7311ca11 flags:# cached_page_views:flags.0?int = WebPage;

authorization#ad01d61d flags:# current:flags.0?true official_app:flags.1?true password_pending:flags.2?true encrypted_requests_disabled:flags.3?true call_requests_disabled:flags.4?true hash:long device_model:string platform:string system_version:string api_id:int app_name:string app_version:string date_created:int date_active:int ip:string country:string region:string = Authorization;

account.authorizations#4bff8ea0 authorization_ttl_days:int authorizations:Vector<Authorization> = account.Authorizations;

account.password#957b50fb flags:# has_recovery:flags.0?true has_secure_values:flags.1?true has_password:flags.2?true current_algo:flags.2?PasswordKdfAlgo srp_B:flags.2?bytes srp_id:flags.2?long hint:flags.3?string email_unconfirmed_pattern:flags.4?string new_algo:PasswordKdfAlgo new_secure_algo:SecurePasswordKdfAlgo secure_random:bytes pending_reset_date:flags.5?int login_email_pattern:flags.6?string = account.Password;

account.passwordSettings#9a5c33e5 flags:# email:flags.0?string secure_settings:flags.1?SecureSecretSettings = account.PasswordSettings;

account.passwordInputSettings#c23727c9 flags:# new_algo:flags.0?PasswordKdfAlgo new_password_hash:flags.0?bytes hint:flags.0?string email:flags.1?string new_secure_settings:flags.2?SecureSecretSettings = account.PasswordInputSettings;

auth.passwordRecovery#137948a5 email_pattern:string = auth.PasswordRecovery;

receivedNotifyMessage#a384b779 id:int flags:int = ReceivedNotifyMessage;

chatInviteExported#ab4a819 flags:# revoked:flags.0?true permanent:flags.5?true request_needed:flags.6?true link:string admin_id:long date:int start_date:flags.4?int expire_date:flags.1?int usage_limit:flags.2?int usage:flags.3?int requested:flags.7?int title:flags.8?string = ExportedChatInvite;
chatInvitePublicJoinRequests#ed107ab7 = ExportedChatInvite;

chatInviteAlready#5a686d7c chat:Chat = ChatInvite;
chatInvite#300c44c1 flags:# channel:flags.0?true broadcast:flags.1?true public:flags.2?true megagroup:flags.3?true request_needed:flags.6?true title:string about:flags.5?string photo:Photo participants_count:int participants:flags.4?Vector<User> = ChatInvite;
chatInvitePeek#61695cb0 chat:Chat expires:int = ChatInvite;

inputStickerSetEmpty#ffb62b95 = InputStickerSet;
inputStickerSetID#9de7a269 id:long access_hash:long = InputStickerSet;
inputStickerSetShortName#861cc8a0 short_name:string = InputStickerSet;
inputStickerSetAnimatedEmoji#28703c8 = InputStickerSet;
inputStickerSetDice#e67f520e emoticon:string = InputStickerSet;
inputStickerSetAnimatedEmojiAnimations#cde3739 = InputStickerSet;
inputStickerSetPremiumGifts#c88b3b02 = InputStickerSet;
inputStickerSetEmojiGenericAnimations#4c4d4ce = InputStickerSet;
inputStickerSetEmojiDefaultStatuses#29d0f5ee = InputStickerSet;

stickerSet#2dd14edc flags:# archived:flags.1?true official:flags.2?true masks:flags.3?true animated:flags.5?true videos:flags.6?true emojis:flags.7?true installed_date:flags.0?int id:long access_hash:long title:string short_name:string thumbs:flags.4?Vector<PhotoSize> thumb_dc_id:flags.4?int thumb_version:flags.4?int thumb_document_id:flags.8?long count:int hash:int = StickerSet;

messages.stickerSet#b60a24a6 set:StickerSet packs:Vector<StickerPack> documents:Vector<Document> = messages.StickerSet;
messages.stickerSetNotModified#d3f924eb = messages.StickerSet;

botCommand#c27ac8c7 command:string description:string = BotCommand;

botInfo#8f300b57 flags:# user_id:flags.0?long description:flags.1?string description_photo:flags.4?Photo description_document:flags.5?Document commands:flags.2?Vector<BotCommand> menu_button:flags.3?BotMenuButton = BotInfo;

keyboardButton#a2fa4880 text:string = KeyboardButton;
keyboardButtonUrl#258aff05 text:string url:string = KeyboardButton;
keyboardButtonCallback#35bbdb6b flags:# requires_password:flags.0?true text:string data:bytes = KeyboardButton;
keyboardButtonRequestPhone#b16a6c29 text:string = KeyboardButton;
keyboardButtonRequestGeoLocation#fc796b3f text:string = KeyboardButton;
keyboardButtonSwitchInline#568a748 flags:# same_peer:flags.0?true text:string query:string = KeyboardButton;
keyboardButtonGame#50f41ccf text:string = KeyboardButton;
keyboardButtonBuy#afd93fbb text:string = KeyboardButton;
keyboardButtonUrlAuth#10b78d29 flags:# text:string fwd_text:flags.0?string url:string button_id:int = KeyboardButton;
inputKeyboardButtonUrlAuth#d02e7fd4 flags:# request_write_access:flags.0?true text:string fwd_text:flags.1?string url:string bot:InputUser = KeyboardButton;
keyboardButtonRequestPoll#bbc7515d flags:# quiz:flags.0?Bool text:string = KeyboardButton;
inputKeyboardButtonUserProfile#e988037b text:string user_id:InputUser = KeyboardButton;
keyboardButtonUserProfile#308660c1 text:string user_id:long = KeyboardButton;
keyboardButtonWebView#13767230 text:string url:string = KeyboardButton;
keyboardButtonSimpleWebView#a0c0505c text:string url:string = KeyboardButton;

keyboardButtonRow#77608b83 buttons:Vector<KeyboardButton> = KeyboardButtonRow;

replyKeyboardHide#a03e5b85 flags:# selective:flags.2?true = ReplyMarkup;
replyKeyboardForceReply#86b40b08 flags:# single_use:flags.1?true selective:flags.2?true placeholder:flags.3?string = ReplyMarkup;
replyKeyboardMarkup#85dd99d1 flags:# resize:flags.0?true single_use:flags.1?true selective:flags.2?true rows:Vector<KeyboardButtonRow> placeholder:flags.3?string = ReplyMarkup;
replyInlineMarkup#48a30254 rows:Vector<KeyboardButtonRow> = ReplyMarkup;

messageEntityUnknown#bb92ba95 offset:int length:int = MessageEntity;
messageEntityMention#fa04579d offset:int length:int = MessageEntity;
messageEntityHashtag#6f635b0d offset:int length:int = MessageEntity;
messageEntityBotCommand#6cef8ac7 offset:int length:int = MessageEntity;
messageEntityUrl#6ed02538 offset:int length:int = MessageEntity;
messageEntityEmail#64e475c2 offset:int length:int = MessageEntity;
messageEntityBold#bd610bc9 offset:int length:int = MessageEntity;
messageEntityItalic#826f8b60 offset:int length:int = MessageEntity;
messageEntityCode#28a20571 offset:int length:int = MessageEntity;
messageEntityPre#73924be0 offset:int length:int language:string = MessageEntity;
messageEntityTextUrl#76a6d327 offset:int length:int url:string = MessageEntity;
messageEntityMentionName#dc7b1140 offset:int length:int user_id:long = MessageEntity;
inputMessageEntityMentionName#208e68c9 offset:int length:int user_id:InputUser = MessageEntity;
messageEntityPhone#9b69e34b offset:int length:int = MessageEntity;
messageEntityCashtag#4c4e743f offset:int length:int = MessageEntity;
messageEntityUnderline#9c4e7e8b offset:int length:int = MessageEntity;
messageEntityStrike#bf0693d4 offset:int length:int = MessageEntity;
messageEntityBlockquote#20df5d0 offset:int length:int = MessageEntity;
messageEntityBankCard#761e6af4 offset:int length:int = MessageEntity;
messageEntitySpoiler#32ca960f offset:int length:int = MessageEntity;
messageEntityCustomEmoji#c8cf05f8 offset:int length:int document_id:long = MessageEntity;

inputChannelEmpty#ee8c1e86 = InputChannel;
inputChannel#f35aec28 channel_id:long access_hash:long = InputChannel;
inputChannelFromMessage#5b934f9d peer:InputPeer msg_id:int channel_id:long = InputChannel;

contacts.resolvedPeer#7f077ad9 peer:Peer chats:Vector<Chat> users:Vector<User> = contacts.ResolvedPeer;

messageRange#ae30253 min_id:int max_id:int = MessageRange;

updates.channelDifferenceEmpty#3e11affb flags:# final:flags.0?true pts:int timeout:flags.1?int = updates.ChannelDifference;
updates.channelDifferenceTooLong#a4bcc6fe flags:# final:flags.0?true timeout:flags.1?int dialog:Dialog messages:Vector<Message> chats:Vector<Chat> users:Vector<User> = updates.ChannelDifference;
updates.channelDifference#2064674e flags:# final:flags.0?true pts:int timeout:flags.1?int new_messages:Vector<Message> other_updates:Vector<Update> chats:Vector<Chat> users:Vector<User> = updates.ChannelDifference;

channelMessagesFilterEmpty#94d42ee7 = ChannelMessagesFilter;
channelMessagesFilter#cd77d957 flags:# exclude_new_messages:flags.1?true ranges:Vector<MessageRange> = ChannelMessagesFilter;

channelParticipant#c00c07c0 user_id:long date:int = ChannelParticipant;
channelParticipantSelf#35a8bfa7 flags:# via_request:flags.0?true user_id:long inviter_id:long date:int = ChannelParticipant;
channelParticipantCreator#2fe601d3 flags:# user_id:long admin_rights:ChatAdminRights rank:flags.0?string = ChannelParticipant;
channelParticipantAdmin#34c3bb53 flags:# can_edit:flags.0?true self:flags.1?true user_id:long inviter_id:flags.1?long promoted_by:long date:int admin_rights:ChatAdminRights rank:flags.2?string = ChannelParticipant;
channelParticipantBanned#6df8014e flags:# left:flags.0?true peer:Peer kicked_by:long date:int banned_rights:ChatBannedRights = ChannelParticipant;
channelParticipantLeft#1b03f006 peer:Peer = ChannelParticipant;

channelParticipantsRecent#de3f3c79 = ChannelParticipantsFilter;
channelParticipantsAdmins#b4608969 = ChannelParticipantsFilter;
channelParticipantsKicked#a3b54985 q:string = ChannelParticipantsFilter;
channelParticipantsBots#b0d1865b = ChannelParticipantsFilter;
channelParticipantsBanned#1427a5e1 q:string = ChannelParticipantsFilter;
channelParticipantsSearch#656ac4b q:string = ChannelParticipantsFilter;
channelParticipantsContacts#bb6ae88d q:string = ChannelParticipantsFilter;
channelParticipantsMentions#e04b5ceb flags:# q:flags.0?string top_msg_id:flags.1?int = ChannelParticipantsFilter;

channels.channelParticipants#9ab0feaf count:int participants:Vector<ChannelParticipant> chats:Vector<Chat> users:Vector<User> = channels.ChannelParticipants;
channels.channelParticipantsNotModified#f0173fe9 = channels.ChannelParticipants;

channels.channelParticipant#dfb80317 participant:ChannelParticipant chats:Vector<Chat> users:Vector<User> = channels.ChannelParticipant;

help.termsOfService#780a0310 flags:# popup:flags.0?true id:DataJSON text:string entities:Vector<MessageEntity> min_age_confirm:flags.1?int = help.TermsOfService;

messages.savedGifsNotModified#e8025ca2 = messages.SavedGifs;
messages.savedGifs#84a02a0d hash:long gifs:Vector<Document> = messages.SavedGifs;

inputBotInlineMessageMediaAuto#3380c786 flags:# message:string entities:flags.1?Vector<MessageEntity> reply_markup:flags.2?ReplyMarkup = InputBotInlineMessage;
inputBotInlineMessageText#3dcd7a87 flags:# no_webpage:flags.0?true message:string entities:flags.1?Vector<MessageEntity> reply_markup:flags.2?ReplyMarkup = InputBotInlineMessage;
inputBotInlineMessageMediaGeo#96929a85 flags:# geo_point:InputGeoPoint heading:flags.0?int period:flags.1?int proximity_notification_radius:flags.3?int reply_markup:flags.2?ReplyMarkup = InputBotInlineMessage;
inputBotInlineMessageMediaVenue#417bbf11 flags:# geo_point:InputGeoPoint title:string address:string provider:string venue_id:string venue_type:string reply_markup:flags.2?ReplyMarkup = InputBotInlineMessage;
inputBotInlineMessageMediaContact#a6edbffd flags:# phone_number:string first_name:string last_name:string vcard:string reply_markup:flags.2?ReplyMarkup = InputBotInlineMessage;
inputBotInlineMessageGame#4b425864 flags:# reply_markup:flags.2?ReplyMarkup = InputBotInlineMessage;
inputBotInlineMessageMediaInvoice#d7e78225 flags:# title:string description:string photo:flags.0?InputWebDocument invoice:Invoice payload:bytes provider:string provider_data:DataJSON reply_markup:flags.2?ReplyMarkup = InputBotInlineMessage;

inputBotInlineResult#88bf9319 flags:# id:string type:string title:flags.1?string description:flags.2?string url:flags.3?string thumb:flags.4?InputWebDocument content:flags.5?InputWebDocument send_message:InputBotInlineMessage = InputBotInlineResult;
inputBotInlineResultPhoto#a8d864a7 id:string type:string photo:InputPhoto send_message:InputBotInlineMessage = InputBotInlineResult;
inputBotInlineResultDocument#fff8fdc4 flags:# id:string type:string title:flags.1?string description:flags.2?string document:InputDocument send_message:InputBotInlineMessage = InputBotInlineResult;
inputBotInlineResultGame#4fa417f2 id:string short_name:string send_message:InputBotInlineMessage = InputBotInlineResult;

botInlineMessageMediaAuto#764cf810 flags:# message:string entities:flags.1?Vector<MessageEntity> reply_markup:flags.2?ReplyMarkup = BotInlineMessage;
botInlineMessageText#8c7f65e2 flags:# no_webpage:flags.0?true message:string entities:flags.1?Vector<MessageEntity> reply_markup:flags.2?ReplyMarkup = BotInlineMessage;
botInlineMessageMediaGeo#51846fd flags:# geo:GeoPoint heading:flags.0?int period:flags.1?int proximity_notification_radius:flags.3?int reply_markup:flags.2?ReplyMarkup = BotInlineMessage;
botInlineMessageMediaVenue#8a86659c flags:# geo:GeoPoint title:string address:string provider:string venue_id:string venue_type:string reply_markup:flags.2?ReplyMarkup = BotInlineMessage;
botInlineMessageMediaContact#18d1cdc2 flags:# phone_number:string first_name:string last_name:string vcard:string reply_markup:flags.2?ReplyMarkup = BotInlineMessage;
botInlineMessageMediaInvoice#354a9b09 flags:# shipping_address_requested:flags.1?true test:flags.3?true title:string description:string photo:flags.0?WebDocument currency:string total_amount:long reply_markup:flags.2?ReplyMarkup = BotInlineMessage;

botInlineResult#11965f3a flags:# id:string type:string title:flags.1?string description:flags.2?string url:flags.3?string thumb:flags.4?WebDocument content:flags.5?WebDocument send_message:BotInlineMessage = BotInlineResult;
botInlineMediaResult#17db940b flags:# id:string type:string photo:flags.0?Photo document:flags.1?Document title:flags.2?string description:flags.3?string send_message:BotInlineMessage = BotInlineResult;

messages.botResults#947ca848 flags:# gallery:flags.0?true query_id:long next_offset:flags.1?string switch_pm:flags.2?InlineBotSwitchPM results:Vector<BotInlineResult> cache_time:int users:Vector<User> = messages.BotResults;

exportedMessageLink#5dab1af4 link:string html:string = ExportedMessageLink;

messageFwdHeader#5f777dce flags:# imported:flags.7?true from_id:flags.0?Peer from_name:flags.5?string date:int channel_post:flags.2?int post_author:flags.3?string saved_from_peer:flags.4?Peer saved_from_msg_id:flags.4?int psa_type:flags.6?string = MessageFwdHeader;

auth.codeTypeSms#72a3158c = auth.CodeType;
auth.codeTypeCall#741cd3e3 = auth.CodeType;
auth.codeTypeFlashCall#226ccefb = auth.CodeType;
auth.codeTypeMissedCall#d61ad6ee = auth.CodeType;

auth.sentCodeTypeApp#3dbb5986 length:int = auth.SentCodeType;
auth.sentCodeTypeSms#c000bba2 length:int = auth.SentCodeType;
auth.sentCodeTypeCall#5353e5a7 length:int = auth.SentCodeType;
auth.sentCodeTypeFlashCall#ab03c6d9 pattern:string = auth.SentCodeType;
auth.sentCodeTypeMissedCall#82006484 prefix:string length:int = auth.SentCodeType;
auth.sentCodeTypeEmailCode#5a159841 flags:# apple_signin_allowed:flags.0?true google_signin_allowed:flags.1?true email_pattern:string length:int next_phone_login_date:flags.2?int = auth.SentCodeType;
auth.sentCodeTypeSetUpEmailRequired#a5491dea flags:# apple_signin_allowed:flags.0?true google_signin_allowed:flags.1?true = auth.SentCodeType;

messages.botCallbackAnswer#36585ea4 flags:# alert:flags.1?true has_url:flags.3?true native_ui:flags.4?true message:flags.0?string url:flags.2?string cache_time:int = messages.BotCallbackAnswer;

messages.messageEditData#26b5dde6 flags:# caption:flags.0?true = messages.MessageEditData;

inputBotInlineMessageID#890c3d89 dc_id:int id:long access_hash:long = InputBotInlineMessageID;
inputBotInlineMessageID64#b6d915d7 dc_id:int owner_id:long id:int access_hash:long = InputBotInlineMessageID;

inlineBotSwitchPM#3c20629f text:string start_param:string = InlineBotSwitchPM;

messages.peerDialogs#3371c354 dialogs:Vector<Dialog> messages:Vector<Message> chats:Vector<Chat> users:Vector<User> state:updates.State = messages.PeerDialogs;

topPeer#edcdc05b peer:Peer rating:double = TopPeer;

topPeerCategoryBotsPM#ab661b5b = TopPeerCategory;
topPeerCategoryBotsInline#148677e2 = TopPeerCategory;
topPeerCategoryCorrespondents#637b7ed = TopPeerCategory;
topPeerCategoryGroups#bd17a14a = TopPeerCategory;
topPeerCategoryChannels#161d9628 = TopPeerCategory;
topPeerCategoryPhoneCalls#1e76a78c = TopPeerCategory;
topPeerCategoryForwardUsers#a8406ca9 = TopPeerCategory;
topPeerCategoryForwardChats#fbeec0f0 = TopPeerCategory;

topPeerCategoryPeers#fb834291 category:TopPeerCategory count:int peers:Vector<TopPeer> = TopPeerCategoryPeers;

contacts.topPeersNotModified#de266ef5 = contacts.TopPeers;
contacts.topPeers#70b772a8 categories:Vector<TopPeerCategoryPeers> chats:Vector<Chat> users:Vector<User> = contacts.TopPeers;
contacts.topPeersDisabled#b52c939d = contacts.TopPeers;

draftMessageEmpty#1b0c841a flags:# date:flags.0?int = DraftMessage;
draftMessage#fd8e711f flags:# no_webpage:flags.1?true reply_to_msg_id:flags.0?int message:string entities:flags.3?Vector<MessageEntity> date:int = DraftMessage;

messages.featuredStickersNotModified#c6dc0c66 count:int = messages.FeaturedStickers;
messages.featuredStickers#be382906 flags:# premium:flags.0?true hash:long count:int sets:Vector<StickerSetCovered> unread:Vector<long> = messages.FeaturedStickers;

messages.recentStickersNotModified#b17f890 = messages.RecentStickers;
messages.recentStickers#88d37c56 hash:long packs:Vector<StickerPack> stickers:Vector<Document> dates:Vector<int> = messages.RecentStickers;

messages.archivedStickers#4fcba9c8 count:int sets:Vector<StickerSetCovered> = messages.ArchivedStickers;

messages.stickerSetInstallResultSuccess#38641628 = messages.StickerSetInstallResult;
messages.stickerSetInstallResultArchive#35e410a8 sets:Vector<StickerSetCovered> = messages.StickerSetInstallResult;

stickerSetCovered#6410a5d2 set:StickerSet cover:Document = StickerSetCovered;
stickerSetMultiCovered#3407e51b set:StickerSet covers:Vector<Document> = StickerSetCovered;
stickerSetFullCovered#1aed5ee5 set:StickerSet packs:Vector<StickerPack> documents:Vector<Document> = StickerSetCovered;

maskCoords#aed6dbb2 n:int x:double y:double zoom:double = MaskCoords;

inputStickeredMediaPhoto#4a992157 id:InputPhoto = InputStickeredMedia;
inputStickeredMediaDocument#438865b id:InputDocument = InputStickeredMedia;

game#bdf9653b flags:# id:long access_hash:long short_name:string title:string description:string photo:Photo document:flags.0?Document = Game;

inputGameID#32c3e77 id:long access_hash:long = InputGame;
inputGameShortName#c331e80a bot_id:InputUser short_name:string = InputGame;

highScore#73a379eb pos:int user_id:long score:int = HighScore;

messages.highScores#9a3bfd99 scores:Vector<HighScore> users:Vector<User> = messages.HighScores;

textEmpty#dc3d824f = RichText;
textPlain#744694e0 text:string = RichText;
textBold#6724abc4 text:RichText = RichText;
textItalic#d912a59c text:RichText = RichText;
textUnderline#c12622c4 text:RichText = RichText;
textStrike#9bf8bb95 text:RichText = RichText;
textFixed#6c3f19b9 text:RichText = RichText;
textUrl#3c2884c1 text:RichText url:string webpage_id:long = RichText;
textEmail#de5a0dd6 text:RichText email:string = RichText;
textConcat#7e6260d7 texts:Vector<RichText> = RichText;
textSubscript#ed6a8504 text:RichText = RichText;
textSuperscript#c7fb5e01 text:RichText = RichText;
textMarked#34b8621 text:RichText = RichText;
textPhone#1ccb966a text:RichText phone:string = RichText;
textImage#81ccf4f document_id:long w:int h:int = RichText;
textAnchor#35553762 text:RichText name:string = RichText;

pageBlockUnsupported#13567e8a = PageBlock;
pageBlockTitle#70abc3fd text:RichText = PageBlock;
pageBlockSubtitle#8ffa9a1f text:RichText = PageBlock;
pageBlockAuthorDate#baafe5e0 author:RichText published_date:int = PageBlock;
pageBlockHeader#bfd064ec text:RichText = PageBlock;
pageBlockSubheader#f12bb6e1 text:RichText = PageBlock;
pageBlockParagraph#467a0766 text:RichText = PageBlock;
pageBlockPreformatted#c070d93e text:RichText language:string = PageBlock;
pageBlockFooter#48870999 text:RichText = PageBlock;
pageBlockDivider#db20b188 = PageBlock;
pageBlockAnchor#ce0d37b0 name:string = PageBlock;
pageBlockList#e4e88011 items:Vector<PageListItem> = PageBlock;
pageBlockBlockquote#263d7c26 text:RichText caption:RichText = PageBlock;
pageBlockPullquote#4f4456d3 text:RichText caption:RichText = PageBlock;
pageBlockPhoto#1759c560 flags:# photo_id:long caption:PageCaption url:flags.0?string webpage_id:flags.0?long = PageBlock;
pageBlockVideo#7c8fe7b6 flags:# autoplay:flags.0?true loop:flags.1?true video_id:long caption:PageCaption = PageBlock;
pageBlockCover#39f23300 cover:PageBlock = PageBlock;
pageBlockEmbed#a8718dc5 flags:# full_width:flags.0?true allow_scrolling:flags.3?true url:flags.1?string html:flags.2?string poster_photo_id:flags.4?long w:flags.5?int h:flags.5?int caption:PageCaption = PageBlock;
pageBlockEmbedPost#f259a80b url:string webpage_id:long author_photo_id:long author:string date:int blocks:Vector<PageBlock> caption:PageCaption = PageBlock;
pageBlockCollage#65a0fa4d items:Vector<PageBlock> caption:PageCaption = PageBlock;
pageBlockSlideshow#31f9590 items:Vector<PageBlock> caption:PageCaption = PageBlock;
pageBlockChannel#ef1751b5 channel:Chat = PageBlock;
pageBlockAudio#804361ea audio_id:long caption:PageCaption = PageBlock;
pageBlockKicker#1e148390 text:RichText = PageBlock;
pageBlockTable#bf4dea82 flags:# bordered:flags.0?true striped:flags.1?true title:RichText rows:Vector<PageTableRow> = PageBlock;
pageBlockOrderedList#9a8ae1e1 items:Vector<PageListOrderedItem> = PageBlock;
pageBlockDetails#76768bed flags:# open:flags.0?true blocks:Vector<PageBlock> title:RichText = PageBlock;
pageBlockRelatedArticles#16115a96 title:RichText articles:Vector<PageRelatedArticle> = PageBlock;
pageBlockMap#a44f3ef6 geo:GeoPoint zoom:int w:int h:int caption:PageCaption = PageBlock;

phoneCallDiscardReasonMissed#85e42301 = PhoneCallDiscardReason;
phoneCallDiscardReasonDisconnect#e095c1a0 = PhoneCallDiscardReason;
phoneCallDiscardReasonHangup#57adc690 = PhoneCallDiscardReason;
phoneCallDiscardReasonBusy#faf7e8c9 = PhoneCallDiscardReason;

dataJSON#7d748d04 data:string = DataJSON;

labeledPrice#cb296bf8 label:string amount:long = LabeledPrice;

invoice#3e85a91b flags:# test:flags.0?true name_requested:flags.1?true phone_requested:flags.2?true email_requested:flags.3?true shipping_address_requested:flags.4?true flexible:flags.5?true phone_to_provider:flags.6?true email_to_provider:flags.7?true recurring:flags.9?true currency:string prices:Vector<LabeledPrice> max_tip_amount:flags.8?long suggested_tip_amounts:flags.8?Vector<long> recurring_terms_url:flags.9?string = Invoice;

paymentCharge#ea02c27e id:string provider_charge_id:string = PaymentCharge;

postAddress#1e8caaeb street_line1:string street_line2:string city:string state:string country_iso2:string post_code:string = PostAddress;

paymentRequestedInfo#909c3f94 flags:# name:flags.0?string phone:flags.1?string email:flags.2?string shipping_address:flags.3?PostAddress = PaymentRequestedInfo;

paymentSavedCredentialsCard#cdc27a1f id:string title:string = PaymentSavedCredentials;

webDocument#1c570ed1 url:string access_hash:long size:int mime_type:string attributes:Vector<DocumentAttribute> = WebDocument;
webDocumentNoProxy#f9c8bcc6 url:string size:int mime_type:string attributes:Vector<DocumentAttribute> = WebDocument;

inputWebDocument#9bed434d url:string size:int mime_type:string attributes:Vector<DocumentAttribute> = InputWebDocument;

inputWebFileLocation#c239d686 url:string access_hash:long = InputWebFileLocation;
inputWebFileGeoPointLocation#9f2221c9 geo_point:InputGeoPoint access_hash:long w:int h:int zoom:int scale:int = InputWebFileLocation;
inputWebFileAudioAlbumThumbLocation#f46fe924 flags:# small:flags.2?true document:flags.0?InputDocument title:flags.1?string performer:flags.1?string = InputWebFileLocation;

upload.webFile#21e753bc size:int mime_type:string file_type:storage.FileType mtime:int bytes:bytes = upload.WebFile;

payments.paymentForm#a0058751 flags:# can_save_credentials:flags.2?true password_missing:flags.3?true form_id:long bot_id:long title:string description:string photo:flags.5?WebDocument invoice:Invoice provider_id:long url:string native_provider:flags.4?string native_params:flags.4?DataJSON additional_methods:flags.6?Vector<PaymentFormMethod> saved_info:flags.0?PaymentRequestedInfo saved_credentials:flags.1?Vector<PaymentSavedCredentials> users:Vector<User> = payments.PaymentForm;

payments.validatedRequestedInfo#d1451883 flags:# id:flags.0?string shipping_options:flags.1?Vector<ShippingOption> = payments.ValidatedRequestedInfo;

payments.paymentResult#4e5f810d updates:Updates = payments.PaymentResult;
payments.paymentVerificationNeeded#d8411139 url:string = payments.PaymentResult;

payments.paymentReceipt#70c4fe03 flags:# date:int bot_id:long provider_id:long title:string description:string photo:flags.2?WebDocument invoice:Invoice info:flags.0?PaymentRequestedInfo shipping:flags.1?ShippingOption tip_amount:flags.3?long currency:string total_amount:long credentials_title:string users:Vector<User> = payments.PaymentReceipt;

payments.savedInfo#fb8fe43c flags:# has_saved_credentials:flags.1?true saved_info:flags.0?PaymentRequestedInfo = payments.SavedInfo;

inputPaymentCredentialsSaved#c10eb2cf id:string tmp_password:bytes = InputPaymentCredentials;
inputPaymentCredentials#3417d728 flags:# save:flags.0?true data:DataJSON = InputPaymentCredentials;
inputPaymentCredentialsApplePay#aa1c39f payment_data:DataJSON = InputPaymentCredentials;
inputPaymentCredentialsGooglePay#8ac32801 payment_token:DataJSON = InputPaymentCredentials;

account.tmpPassword#db64fd34 tmp_password:bytes valid_until:int = account.TmpPassword;

shippingOption#b6213cdf id:string title:string prices:Vector<LabeledPrice> = ShippingOption;

inputStickerSetItem#ffa0a496 flags:# document:InputDocument emoji:string mask_coords:flags.0?MaskCoords = InputStickerSetItem;

inputPhoneCall#1e36fded id:long access_hash:long = InputPhoneCall;

phoneCallEmpty#5366c915 id:long = PhoneCall;
phoneCallWaiting#c5226f17 flags:# video:flags.6?true id:long access_hash:long date:int admin_id:long participant_id:long protocol:PhoneCallProtocol receive_date:flags.0?int = PhoneCall;
phoneCallRequested#14b0ed0c flags:# video:flags.6?true id:long access_hash:long date:int admin_id:long participant_id:long g_a_hash:bytes protocol:PhoneCallProtocol = PhoneCall;
phoneCallAccepted#3660c311 flags:# video:flags.6?true id:long access_hash:long date:int admin_id:long participant_id:long g_b:bytes protocol:PhoneCallProtocol = PhoneCall;
phoneCall#967f7c67 flags:# p2p_allowed:flags.5?true video:flags.6?true id:long access_hash:long date:int admin_id:long participant_id:long g_a_or_b:bytes key_fingerprint:long protocol:PhoneCallProtocol connections:Vector<PhoneConnection> start_date:int = PhoneCall;
phoneCallDiscarded#50ca4de1 flags:# need_rating:flags.2?true need_debug:flags.3?true video:flags.6?true id:long reason:flags.0?PhoneCallDiscardReason duration:flags.1?int = PhoneCall;

phoneConnection#9cc123c7 flags:# tcp:flags.0?true id:long ip:string ipv6:string port:int peer_tag:bytes = PhoneConnection;
phoneConnectionWebrtc#635fe375 flags:# turn:flags.0?true stun:flags.1?true id:long ip:string ipv6:string port:int username:string password:string = PhoneConnection;

phoneCallProtocol#fc878fc8 flags:# udp_p2p:flags.0?true udp_reflector:flags.1?true min_layer:int max_layer:int library_versions:Vector<string> = PhoneCallProtocol;

phone.phoneCall#ec82e140 phone_call:PhoneCall users:Vector<User> = phone.PhoneCall;

upload.cdnFileReuploadNeeded#eea8e46e request_token:bytes = upload.CdnFile;
upload.cdnFile#a99fca4f bytes:bytes = upload.CdnFile;

cdnPublicKey#c982eaba dc_id:int public_key:string = CdnPublicKey;

cdnConfig#5725e40a public_keys:Vector<CdnPublicKey> = CdnConfig;

langPackString#cad181f6 key:string value:string = LangPackString;
langPackStringPluralized#6c47ac9f flags:# key:string zero_value:flags.0?string one_value:flags.1?string two_value:flags.2?string few_value:flags.3?string many_value:flags.4?string other_value:string = LangPackString;
langPackStringDeleted#2979eeb2 key:string = LangPackString;

langPackDifference#f385c1f6 lang_code:string from_version:int version:int strings:Vector<LangPackString> = LangPackDifference;

langPackLanguage#eeca5ce3 flags:# official:flags.0?true rtl:flags.2?true beta:flags.3?true name:string native_name:string lang_code:string base_lang_code:flags.1?string plural_code:string strings_count:int translated_count:int translations_url:string = LangPackLanguage;

channelAdminLogEventActionChangeTitle#e6dfb825 prev_value:string new_value:string = ChannelAdminLogEventAction;
channelAdminLogEventActionChangeAbout#55188a2e prev_value:string new_value:string = ChannelAdminLogEventAction;
channelAdminLogEventActionChangeUsername#6a4afc38 prev_value:string new_value:string = ChannelAdminLogEventAction;
channelAdminLogEventActionChangePhoto#434bd2af prev_photo:Photo new_photo:Photo = ChannelAdminLogEventAction;
channelAdminLogEventActionToggleInvites#1b7907ae new_value:Bool = ChannelAdminLogEventAction;
channelAdminLogEventActionToggleSignatures#26ae0971 new_value:Bool = ChannelAdminLogEventAction;
channelAdminLogEventActionUpdatePinned#e9e82c18 message:Message = ChannelAdminLogEventAction;
channelAdminLogEventActionEditMessage#709b2405 prev_message:Message new_message:Message = ChannelAdminLogEventAction;
channelAdminLogEventActionDeleteMessage#42e047bb message:Message = ChannelAdminLogEventAction;
channelAdminLogEventActionParticipantJoin#183040d3 = ChannelAdminLogEventAction;
channelAdminLogEventActionParticipantLeave#f89777f2 = ChannelAdminLogEventAction;
channelAdminLogEventActionParticipantInvite#e31c34d8 participant:ChannelParticipant = ChannelAdminLogEventAction;
channelAdminLogEventActionParticipantToggleBan#e6d83d7e prev_participant:ChannelParticipant new_participant:ChannelParticipant = ChannelAdminLogEventAction;
channelAdminLogEventActionParticipantToggleAdmin#d5676710 prev_participant:ChannelParticipant new_participant:ChannelParticipant = ChannelAdminLogEventAction;
channelAdminLogEventActionChangeStickerSet#b1c3caa7 prev_stickerset:InputStickerSet new_stickerset:InputStickerSet = ChannelAdminLogEventAction;
channelAdminLogEventActionTogglePreHistoryHidden#5f5c95f1 new_value:Bool = ChannelAdminLogEventAction;
channelAdminLogEventActionDefaultBannedRights#2df5fc0a prev_banned_rights:ChatBannedRights new_banned_rights:ChatBannedRights = ChannelAdminLogEventAction;
channelAdminLogEventActionStopPoll#8f079643 message:Message = ChannelAdminLogEventAction;
channelAdminLogEventActionChangeLinkedChat#50c7ac8 prev_value:long new_value:long = ChannelAdminLogEventAction;
channelAdminLogEventActionChangeLocation#e6b76ae prev_value:ChannelLocation new_value:ChannelLocation = ChannelAdminLogEventAction;
channelAdminLogEventActionToggleSlowMode#53909779 prev_value:int new_value:int = ChannelAdminLogEventAction;
channelAdminLogEventActionStartGroupCall#23209745 call:InputGroupCall = ChannelAdminLogEventAction;
channelAdminLogEventActionDiscardGroupCall#db9f9140 call:InputGroupCall = ChannelAdminLogEventAction;
channelAdminLogEventActionParticipantMute#f92424d2 participant:GroupCallParticipant = ChannelAdminLogEventAction;
channelAdminLogEventActionParticipantUnmute#e64429c0 participant:GroupCallParticipant = ChannelAdminLogEventAction;
channelAdminLogEventActionToggleGroupCallSetting#56d6a247 join_muted:Bool = ChannelAdminLogEventAction;
channelAdminLogEventActionParticipantJoinByInvite#5cdada77 invite:ExportedChatInvite = ChannelAdminLogEventAction;
channelAdminLogEventActionExportedInviteDelete#5a50fca4 invite:ExportedChatInvite = ChannelAdminLogEventAction;
channelAdminLogEventActionExportedInviteRevoke#410a134e invite:ExportedChatInvite = ChannelAdminLogEventAction;
channelAdminLogEventActionExportedInviteEdit#e90ebb59 prev_invite:ExportedChatInvite new_invite:ExportedChatInvite = ChannelAdminLogEventAction;
channelAdminLogEventActionParticipantVolume#3e7f6847 participant:GroupCallParticipant = ChannelAdminLogEventAction;
channelAdminLogEventActionChangeHistoryTTL#6e941a38 prev_value:int new_value:int = ChannelAdminLogEventAction;
channelAdminLogEventActionParticipantJoinByRequest#afb6144a invite:ExportedChatInvite approved_by:long = ChannelAdminLogEventAction;
channelAdminLogEventActionToggleNoForwards#cb2ac766 new_value:Bool = ChannelAdminLogEventAction;
channelAdminLogEventActionSendMessage#278f2868 message:Message = ChannelAdminLogEventAction;
channelAdminLogEventActionChangeAvailableReactions#be4e0ef8 prev_value:ChatReactions new_value:ChatReactions = ChannelAdminLogEventAction;

channelAdminLogEvent#1fad68cd id:long date:int user_id:long action:ChannelAdminLogEventAction = ChannelAdminLogEvent;

channels.adminLogResults#ed8af74d events:Vector<ChannelAdminLogEvent> chats:Vector<Chat> users:Vector<User> = channels.AdminLogResults;

channelAdminLogEventsFilter#ea107ae4 flags:# join:flags.0?true leave:flags.1?true invite:flags.2?true ban:flags.3?true unban:flags.4?true kick:flags.5?true unkick:flags.6?true promote:flags.7?true demote:flags.8?true info:flags.9?true settings:flags.10?true pinned:flags.11?true edit:flags.12?true delete:flags.13?true group_call:flags.14?true invites:flags.15?true send:flags.16?true = ChannelAdminLogEventsFilter;

popularContact#5ce14175 client_id:long importers:int = PopularContact;

messages.favedStickersNotModified#9e8fa6d3 = messages.FavedStickers;
messages.favedStickers#2cb51097 hash:long packs:Vector<StickerPack> stickers:Vector<Document> = messages.FavedStickers;

recentMeUrlUnknown#46e1d13d url:string = RecentMeUrl;
recentMeUrlUser#b92c09e2 url:string user_id:long = RecentMeUrl;
recentMeUrlChat#b2da71d2 url:string chat_id:long = RecentMeUrl;
recentMeUrlChatInvite#eb49081d url:string chat_invite:ChatInvite = RecentMeUrl;
recentMeUrlStickerSet#bc0a57dc url:string set:StickerSetCovered = RecentMeUrl;

help.recentMeUrls#e0310d7 urls:Vector<RecentMeUrl> chats:Vector<Chat> users:Vector<User> = help.RecentMeUrls;

inputSingleMedia#1cc6e91f flags:# media:InputMedia random_id:long message:string entities:flags.0?Vector<MessageEntity> = InputSingleMedia;

webAuthorization#a6f8f452 hash:long bot_id:long domain:string browser:string platform:string date_created:int date_active:int ip:string region:string = WebAuthorization;

account.webAuthorizations#ed56c9fc authorizations:Vector<WebAuthorization> users:Vector<User> = account.WebAuthorizations;

inputMessageID#a676a322 id:int = InputMessage;
inputMessageReplyTo#bad88395 id:int = InputMessage;
inputMessagePinned#86872538 = InputMessage;
inputMessageCallbackQuery#acfa1a7e id:int query_id:long = InputMessage;

inputDialogPeer#fcaafeb7 peer:InputPeer = InputDialogPeer;
inputDialogPeerFolder#64600527 folder_id:int = InputDialogPeer;

dialogPeer#e56dbf05 peer:Peer = DialogPeer;
dialogPeerFolder#514519e2 folder_id:int = DialogPeer;

messages.foundStickerSetsNotModified#d54b65d = messages.FoundStickerSets;
messages.foundStickerSets#8af09dd2 hash:long sets:Vector<StickerSetCovered> = messages.FoundStickerSets;

fileHash#f39b035c offset:long limit:int hash:bytes = FileHash;

inputClientProxy#75588b3f address:string port:int = InputClientProxy;

help.termsOfServiceUpdateEmpty#e3309f7f expires:int = help.TermsOfServiceUpdate;
help.termsOfServiceUpdate#28ecf961 expires:int terms_of_service:help.TermsOfService = help.TermsOfServiceUpdate;

inputSecureFileUploaded#3334b0f0 id:long parts:int md5_checksum:string file_hash:bytes secret:bytes = InputSecureFile;
inputSecureFile#5367e5be id:long access_hash:long = InputSecureFile;

secureFileEmpty#64199744 = SecureFile;
secureFile#7d09c27e id:long access_hash:long size:long dc_id:int date:int file_hash:bytes secret:bytes = SecureFile;

secureData#8aeabec3 data:bytes data_hash:bytes secret:bytes = SecureData;

securePlainPhone#7d6099dd phone:string = SecurePlainData;
securePlainEmail#21ec5a5f email:string = SecurePlainData;

secureValueTypePersonalDetails#9d2a81e3 = SecureValueType;
secureValueTypePassport#3dac6a00 = SecureValueType;
secureValueTypeDriverLicense#6e425c4 = SecureValueType;
secureValueTypeIdentityCard#a0d0744b = SecureValueType;
secureValueTypeInternalPassport#99a48f23 = SecureValueType;
secureValueTypeAddress#cbe31e26 = SecureValueType;
secureValueTypeUtilityBill#fc36954e = SecureValueType;
secureValueTypeBankStatement#89137c0d = SecureValueType;
secureValueTypeRentalAgreement#8b883488 = SecureValueType;
secureValueTypePassportRegistration#99e3806a = SecureValueType;
secureValueTypeTemporaryRegistration#ea02ec33 = SecureValueType;
secureValueTypePhone#b320aadb = SecureValueType;
secureValueTypeEmail#8e3ca7ee = SecureValueType;

secureValue#187fa0ca flags:# type:SecureValueType data:flags.0?SecureData front_side:flags.1?SecureFile reverse_side:flags.2?SecureFile selfie:flags.3?SecureFile translation:flags.6?Vector<SecureFile> files:flags.4?Vector<SecureFile> plain_data:flags.5?SecurePlainData hash:bytes = SecureValue;

inputSecureValue#db21d0a7 flags:# type:SecureValueType data:flags.0?SecureData front_side:flags.1?InputSecureFile reverse_side:flags.2?InputSecureFile selfie:flags.3?InputSecureFile translation:flags.6?Vector<InputSecureFile> files:flags.4?Vector<InputSecureFile> plain_data:flags.5?SecurePlainData = InputSecureValue;

secureValueHash#ed1ecdb0 type:SecureValueType hash:bytes = SecureValueHash;

secureValueErrorData#e8a40bd9 type:SecureValueType data_hash:bytes field:string text:string = SecureValueError;
secureValueErrorFrontSide#be3dfa type:SecureValueType file_hash:bytes text:string = SecureValueError;
secureValueErrorReverseSide#868a2aa5 type:SecureValueType file_hash:bytes text:string = SecureValueError;
secureValueErrorSelfie#e537ced6 type:SecureValueType file_hash:bytes text:string = SecureValueError;
secureValueErrorFile#7a700873 type:SecureValueType file_hash:bytes text:string = SecureValueError;
secureValueErrorFiles#666220e9 type:SecureValueType file_hash:Vector<bytes> text:string = SecureValueError;
secureValueError#869d758f type:SecureValueType hash:bytes text:string = SecureValueError;
secureValueErrorTranslationFile#a1144770 type:SecureValueType file_hash:bytes text:string = SecureValueError;
secureValueErrorTranslationFiles#34636dd8 type:SecureValueType file_hash:Vector<bytes> text:string = SecureValueError;

secureCredentialsEncrypted#33f0ea47 data:bytes hash:bytes secret:bytes = SecureCredentialsEncrypted;

account.authorizationForm#ad2e1cd8 flags:# required_types:Vector<SecureRequiredType> values:Vector<SecureValue> errors:Vector<SecureValueError> users:Vector<User> privacy_policy_url:flags.0?string = account.AuthorizationForm;

account.sentEmailCode#811f854f email_pattern:string length:int = account.SentEmailCode;

help.deepLinkInfoEmpty#66afa166 = help.DeepLinkInfo;
help.deepLinkInfo#6a4ee832 flags:# update_app:flags.0?true message:string entities:flags.1?Vector<MessageEntity> = help.DeepLinkInfo;

savedPhoneContact#1142bd56 phone:string first_name:string last_name:string date:int = SavedContact;

account.takeout#4dba4501 id:long = account.Takeout;

passwordKdfAlgoUnknown#d45ab096 = PasswordKdfAlgo;
passwordKdfAlgoSHA256SHA256PBKDF2HMACSHA512iter100000SHA256ModPow#3a912d4a salt1:bytes salt2:bytes g:int p:bytes = PasswordKdfAlgo;

securePasswordKdfAlgoUnknown#4a8537 = SecurePasswordKdfAlgo;
securePasswordKdfAlgoPBKDF2HMACSHA512iter100000#bbf2dda0 salt:bytes = SecurePasswordKdfAlgo;
securePasswordKdfAlgoSHA512#86471d92 salt:bytes = SecurePasswordKdfAlgo;

secureSecretSettings#1527bcac secure_algo:SecurePasswordKdfAlgo secure_secret:bytes secure_secret_id:long = SecureSecretSettings;

inputCheckPasswordEmpty#9880f658 = InputCheckPasswordSRP;
inputCheckPasswordSRP#d27ff082 srp_id:long A:bytes M1:bytes = InputCheckPasswordSRP;

secureRequiredType#829d99da flags:# native_names:flags.0?true selfie_required:flags.1?true translation_required:flags.2?true type:SecureValueType = SecureRequiredType;
secureRequiredTypeOneOf#27477b4 types:Vector<SecureRequiredType> = SecureRequiredType;

help.passportConfigNotModified#bfb9f457 = help.PassportConfig;
help.passportConfig#a098d6af hash:int countries_langs:DataJSON = help.PassportConfig;

inputAppEvent#1d1b1245 time:double type:string peer:long data:JSONValue = InputAppEvent;

jsonObjectValue#c0de1bd9 key:string value:JSONValue = JSONObjectValue;

jsonNull#3f6d7b68 = JSONValue;
jsonBool#c7345e6a value:Bool = JSONValue;
jsonNumber#2be0dfa4 value:double = JSONValue;
jsonString#b71e767a value:string = JSONValue;
jsonArray#f7444763 value:Vector<JSONValue> = JSONValue;
jsonObject#99c1d49d value:Vector<JSONObjectValue> = JSONValue;

pageTableCell#34566b6a flags:# header:flags.0?true align_center:flags.3?true align_right:flags.4?true valign_middle:flags.5?true valign_bottom:flags.6?true text:flags.7?RichText colspan:flags.1?int rowspan:flags.2?int = PageTableCell;

pageTableRow#e0c0c5e5 cells:Vector<PageTableCell> = PageTableRow;

pageCaption#6f747657 text:RichText credit:RichText = PageCaption;

pageListItemText#b92fb6cd text:RichText = PageListItem;
pageListItemBlocks#25e073fc blocks:Vector<PageBlock> = PageListItem;

pageListOrderedItemText#5e068047 num:string text:RichText = PageListOrderedItem;
pageListOrderedItemBlocks#98dd8936 num:string blocks:Vector<PageBlock> = PageListOrderedItem;

pageRelatedArticle#b390dc08 flags:# url:string webpage_id:long title:flags.0?string description:flags.1?string photo_id:flags.2?long author:flags.3?string published_date:flags.4?int = PageRelatedArticle;

page#98657f0d flags:# part:flags.0?true rtl:flags.1?true v2:flags.2?true url:string blocks:Vector<PageBlock> photos:Vector<Photo> documents:Vector<Document> views:flags.3?int = Page;

help.supportName#8c05f1c9 name:string = help.SupportName;

help.userInfoEmpty#f3ae2eed = help.UserInfo;
help.userInfo#1eb3758 message:string entities:Vector<MessageEntity> author:string date:int = help.UserInfo;

pollAnswer#6ca9c2e9 text:string option:bytes = PollAnswer;

poll#86e18161 id:long flags:# closed:flags.0?true public_voters:flags.1?true multiple_choice:flags.2?true quiz:flags.3?true question:string answers:Vector<PollAnswer> close_period:flags.4?int close_date:flags.5?int = Poll;

pollAnswerVoters#3b6ddad2 flags:# chosen:flags.0?true correct:flags.1?true option:bytes voters:int = PollAnswerVoters;

pollResults#dcb82ea3 flags:# min:flags.0?true results:flags.1?Vector<PollAnswerVoters> total_voters:flags.2?int recent_voters:flags.3?Vector<long> solution:flags.4?string solution_entities:flags.4?Vector<MessageEntity> = PollResults;

chatOnlines#f041e250 onlines:int = ChatOnlines;

statsURL#47a971e0 url:string = StatsURL;

chatAdminRights#5fb224d5 flags:# change_info:flags.0?true post_messages:flags.1?true edit_messages:flags.2?true delete_messages:flags.3?true ban_users:flags.4?true invite_users:flags.5?true pin_messages:flags.7?true add_admins:flags.9?true anonymous:flags.10?true manage_call:flags.11?true other:flags.12?true = ChatAdminRights;

chatBannedRights#9f120418 flags:# view_messages:flags.0?true send_messages:flags.1?true send_media:flags.2?true send_stickers:flags.3?true send_gifs:flags.4?true send_games:flags.5?true send_inline:flags.6?true embed_links:flags.7?true send_polls:flags.8?true change_info:flags.10?true invite_users:flags.15?true pin_messages:flags.17?true until_date:int = ChatBannedRights;

inputWallPaper#e630b979 id:long access_hash:long = InputWallPaper;
inputWallPaperSlug#72091c80 slug:string = InputWallPaper;
inputWallPaperNoFile#967a462e id:long = InputWallPaper;

account.wallPapersNotModified#1c199183 = account.WallPapers;
account.wallPapers#cdc3858c hash:long wallpapers:Vector<WallPaper> = account.WallPapers;

codeSettings#8a6469c2 flags:# allow_flashcall:flags.0?true current_number:flags.1?true allow_app_hash:flags.4?true allow_missed_call:flags.5?true logout_tokens:flags.6?Vector<bytes> = CodeSettings;

wallPaperSettings#1dc1bca4 flags:# blur:flags.1?true motion:flags.2?true background_color:flags.0?int second_background_color:flags.4?int third_background_color:flags.5?int fourth_background_color:flags.6?int intensity:flags.3?int rotation:flags.4?int = WallPaperSettings;

autoDownloadSettings#8efab953 flags:# disabled:flags.0?true video_preload_large:flags.1?true audio_preload_next:flags.2?true phonecalls_less_data:flags.3?true photo_size_max:int video_size_max:long file_size_max:long video_upload_maxbitrate:int = AutoDownloadSettings;

account.autoDownloadSettings#63cacf26 low:AutoDownloadSettings medium:AutoDownloadSettings high:AutoDownloadSettings = account.AutoDownloadSettings;

emojiKeyword#d5b3b9f9 keyword:string emoticons:Vector<string> = EmojiKeyword;
emojiKeywordDeleted#236df622 keyword:string emoticons:Vector<string> = EmojiKeyword;

emojiKeywordsDifference#5cc761bd lang_code:string from_version:int version:int keywords:Vector<EmojiKeyword> = EmojiKeywordsDifference;

emojiURL#a575739d url:string = EmojiURL;

emojiLanguage#b3fb5361 lang_code:string = EmojiLanguage;

folder#ff544e65 flags:# autofill_new_broadcasts:flags.0?true autofill_public_groups:flags.1?true autofill_new_correspondents:flags.2?true id:int title:string photo:flags.3?ChatPhoto = Folder;

inputFolderPeer#fbd2c296 peer:InputPeer folder_id:int = InputFolderPeer;

folderPeer#e9baa668 peer:Peer folder_id:int = FolderPeer;

messages.searchCounter#e844ebff flags:# inexact:flags.1?true filter:MessagesFilter count:int = messages.SearchCounter;

urlAuthResultRequest#92d33a0e flags:# request_write_access:flags.0?true bot:User domain:string = UrlAuthResult;
urlAuthResultAccepted#8f8c0e4e url:string = UrlAuthResult;
urlAuthResultDefault#a9d6db1f = UrlAuthResult;

channelLocationEmpty#bfb5ad8b = ChannelLocation;
channelLocation#209b82db geo_point:GeoPoint address:string = ChannelLocation;

peerLocated#ca461b5d peer:Peer expires:int distance:int = PeerLocated;
peerSelfLocated#f8ec284b expires:int = PeerLocated;

restrictionReason#d072acb4 platform:string reason:string text:string = RestrictionReason;

inputTheme#3c5693e9 id:long access_hash:long = InputTheme;
inputThemeSlug#f5890df1 slug:string = InputTheme;

theme#a00e67d6 flags:# creator:flags.0?true default:flags.1?true for_chat:flags.5?true id:long access_hash:long slug:string title:string document:flags.2?Document settings:flags.3?Vector<ThemeSettings> emoticon:flags.6?string installs_count:flags.4?int = Theme;

account.themesNotModified#f41eb622 = account.Themes;
account.themes#9a3d8c6d hash:long themes:Vector<Theme> = account.Themes;

auth.loginToken#629f1980 expires:int token:bytes = auth.LoginToken;
auth.loginTokenMigrateTo#68e9916 dc_id:int token:bytes = auth.LoginToken;
auth.loginTokenSuccess#390d5c5e authorization:auth.Authorization = auth.LoginToken;

account.contentSettings#57e28221 flags:# sensitive_enabled:flags.0?true sensitive_can_change:flags.1?true = account.ContentSettings;

messages.inactiveChats#a927fec5 dates:Vector<int> chats:Vector<Chat> users:Vector<User> = messages.InactiveChats;

baseThemeClassic#c3a12462 = BaseTheme;
baseThemeDay#fbd81688 = BaseTheme;
baseThemeNight#b7b31ea8 = BaseTheme;
baseThemeTinted#6d5f77ee = BaseTheme;
baseThemeArctic#5b11125a = BaseTheme;

inputThemeSettings#8fde504f flags:# message_colors_animated:flags.2?true base_theme:BaseTheme accent_color:int outbox_accent_color:flags.3?int message_colors:flags.0?Vector<int> wallpaper:flags.1?InputWallPaper wallpaper_settings:flags.1?WallPaperSettings = InputThemeSettings;

themeSettings#fa58b6d4 flags:# message_colors_animated:flags.2?true base_theme:BaseTheme accent_color:int outbox_accent_color:flags.3?int message_colors:flags.0?Vector<int> wallpaper:flags.1?WallPaper = ThemeSettings;

webPageAttributeTheme#54b56617 flags:# documents:flags.0?Vector<Document> settings:flags.1?ThemeSettings = WebPageAttribute;

messageUserVote#34d247b4 user_id:long option:bytes date:int = MessageUserVote;
messageUserVoteInputOption#3ca5b0ec user_id:long date:int = MessageUserVote;
messageUserVoteMultiple#8a65e557 user_id:long options:Vector<bytes> date:int = MessageUserVote;

messages.votesList#823f649 flags:# count:int votes:Vector<MessageUserVote> users:Vector<User> next_offset:flags.0?string = messages.VotesList;

bankCardOpenUrl#f568028a url:string name:string = BankCardOpenUrl;

payments.bankCardData#3e24e573 title:string open_urls:Vector<BankCardOpenUrl> = payments.BankCardData;

dialogFilter#7438f7e8 flags:# contacts:flags.0?true non_contacts:flags.1?true groups:flags.2?true broadcasts:flags.3?true bots:flags.4?true exclude_muted:flags.11?true exclude_read:flags.12?true exclude_archived:flags.13?true id:int title:string emoticon:flags.25?string pinned_peers:Vector<InputPeer> include_peers:Vector<InputPeer> exclude_peers:Vector<InputPeer> = DialogFilter;
dialogFilterDefault#363293ae = DialogFilter;

dialogFilterSuggested#77744d4a filter:DialogFilter description:string = DialogFilterSuggested;

statsDateRangeDays#b637edaf min_date:int max_date:int = StatsDateRangeDays;

statsAbsValueAndPrev#cb43acde current:double previous:double = StatsAbsValueAndPrev;

statsPercentValue#cbce2fe0 part:double total:double = StatsPercentValue;

statsGraphAsync#4a27eb2d token:string = StatsGraph;
statsGraphError#bedc9822 error:string = StatsGraph;
statsGraph#8ea464b6 flags:# json:DataJSON zoom_token:flags.0?string = StatsGraph;

messageInteractionCounters#ad4fc9bd msg_id:int views:int forwards:int = MessageInteractionCounters;

stats.broadcastStats#bdf78394 period:StatsDateRangeDays followers:StatsAbsValueAndPrev views_per_post:StatsAbsValueAndPrev shares_per_post:StatsAbsValueAndPrev enabled_notifications:StatsPercentValue growth_graph:StatsGraph followers_graph:StatsGraph mute_graph:StatsGraph top_hours_graph:StatsGraph interactions_graph:StatsGraph iv_interactions_graph:StatsGraph views_by_source_graph:StatsGraph new_followers_by_source_graph:StatsGraph languages_graph:StatsGraph recent_message_interactions:Vector<MessageInteractionCounters> = stats.BroadcastStats;

help.promoDataEmpty#98f6ac75 expires:int = help.PromoData;
help.promoData#8c39793f flags:# proxy:flags.0?true expires:int peer:Peer chats:Vector<Chat> users:Vector<User> psa_type:flags.1?string psa_message:flags.2?string = help.PromoData;

videoSize#de33b094 flags:# type:string w:int h:int size:int video_start_ts:flags.0?double = VideoSize;

statsGroupTopPoster#9d04af9b user_id:long messages:int avg_chars:int = StatsGroupTopPoster;

statsGroupTopAdmin#d7584c87 user_id:long deleted:int kicked:int banned:int = StatsGroupTopAdmin;

statsGroupTopInviter#535f779d user_id:long invitations:int = StatsGroupTopInviter;

stats.megagroupStats#ef7ff916 period:StatsDateRangeDays members:StatsAbsValueAndPrev messages:StatsAbsValueAndPrev viewers:StatsAbsValueAndPrev posters:StatsAbsValueAndPrev growth_graph:StatsGraph members_graph:StatsGraph new_members_by_source_graph:StatsGraph languages_graph:StatsGraph messages_graph:StatsGraph actions_graph:StatsGraph top_hours_graph:StatsGraph weekdays_graph:StatsGraph top_posters:Vector<StatsGroupTopPoster> top_admins:Vector<StatsGroupTopAdmin> top_inviters:Vector<StatsGroupTopInviter> users:Vector<User> = stats.MegagroupStats;

globalPrivacySettings#bea2f424 flags:# archive_and_mute_new_noncontact_peers:flags.0?Bool = GlobalPrivacySettings;

help.countryCode#4203c5ef flags:# country_code:string prefixes:flags.0?Vector<string> patterns:flags.1?Vector<string> = help.CountryCode;

help.country#c3878e23 flags:# hidden:flags.0?true iso2:string default_name:string name:flags.1?string country_codes:Vector<help.CountryCode> = help.Country;

help.countriesListNotModified#93cc1f32 = help.CountriesList;
help.countriesList#87d0759e countries:Vector<help.Country> hash:int = help.CountriesList;

messageViews#455b853d flags:# views:flags.0?int forwards:flags.1?int replies:flags.2?MessageReplies = MessageViews;

messages.messageViews#b6c4f543 views:Vector<MessageViews> chats:Vector<Chat> users:Vector<User> = messages.MessageViews;

messages.discussionMessage#a6341782 flags:# messages:Vector<Message> max_id:flags.0?int read_inbox_max_id:flags.1?int read_outbox_max_id:flags.2?int unread_count:int chats:Vector<Chat> users:Vector<User> = messages.DiscussionMessage;

messageReplyHeader#a6d57763 flags:# reply_to_scheduled:flags.2?true reply_to_msg_id:int reply_to_peer_id:flags.0?Peer reply_to_top_id:flags.1?int = MessageReplyHeader;

messageReplies#83d60fc2 flags:# comments:flags.0?true replies:int replies_pts:int recent_repliers:flags.1?Vector<Peer> channel_id:flags.0?long max_id:flags.2?int read_max_id:flags.3?int = MessageReplies;

peerBlocked#e8fd8014 peer_id:Peer date:int = PeerBlocked;

stats.messageStats#8999f295 views_graph:StatsGraph = stats.MessageStats;

groupCallDiscarded#7780bcb4 id:long access_hash:long duration:int = GroupCall;
groupCall#d597650c flags:# join_muted:flags.1?true can_change_join_muted:flags.2?true join_date_asc:flags.6?true schedule_start_subscribed:flags.8?true can_start_video:flags.9?true record_video_active:flags.11?true rtmp_stream:flags.12?true listeners_hidden:flags.13?true id:long access_hash:long participants_count:int title:flags.3?string stream_dc_id:flags.4?int record_start_date:flags.5?int schedule_date:flags.7?int unmuted_video_count:flags.10?int unmuted_video_limit:int version:int = GroupCall;

inputGroupCall#d8aa840f id:long access_hash:long = InputGroupCall;

groupCallParticipant#eba636fe flags:# muted:flags.0?true left:flags.1?true can_self_unmute:flags.2?true just_joined:flags.4?true versioned:flags.5?true min:flags.8?true muted_by_you:flags.9?true volume_by_admin:flags.10?true self:flags.12?true video_joined:flags.15?true peer:Peer date:int active_date:flags.3?int source:int volume:flags.7?int about:flags.11?string raise_hand_rating:flags.13?long video:flags.6?GroupCallParticipantVideo presentation:flags.14?GroupCallParticipantVideo = GroupCallParticipant;

phone.groupCall#9e727aad call:GroupCall participants:Vector<GroupCallParticipant> participants_next_offset:string chats:Vector<Chat> users:Vector<User> = phone.GroupCall;

phone.groupParticipants#f47751b6 count:int participants:Vector<GroupCallParticipant> next_offset:string chats:Vector<Chat> users:Vector<User> version:int = phone.GroupParticipants;

inlineQueryPeerTypeSameBotPM#3081ed9d = InlineQueryPeerType;
inlineQueryPeerTypePM#833c0fac = InlineQueryPeerType;
inlineQueryPeerTypeChat#d766c50a = InlineQueryPeerType;
inlineQueryPeerTypeMegagroup#5ec4be43 = InlineQueryPeerType;
inlineQueryPeerTypeBroadcast#6334ee9a = InlineQueryPeerType;

messages.historyImport#1662af0b id:long = messages.HistoryImport;

messages.historyImportParsed#5e0fb7b9 flags:# pm:flags.0?true group:flags.1?true title:flags.2?string = messages.HistoryImportParsed;

messages.affectedFoundMessages#ef8d3e6c pts:int pts_count:int offset:int messages:Vector<int> = messages.AffectedFoundMessages;

chatInviteImporter#8c5adfd9 flags:# requested:flags.0?true user_id:long date:int about:flags.2?string approved_by:flags.1?long = ChatInviteImporter;

messages.exportedChatInvites#bdc62dcc count:int invites:Vector<ExportedChatInvite> users:Vector<User> = messages.ExportedChatInvites;

messages.exportedChatInvite#1871be50 invite:ExportedChatInvite users:Vector<User> = messages.ExportedChatInvite;
messages.exportedChatInviteReplaced#222600ef invite:ExportedChatInvite new_invite:ExportedChatInvite users:Vector<User> = messages.ExportedChatInvite;

messages.chatInviteImporters#81b6b00a count:int importers:Vector<ChatInviteImporter> users:Vector<User> = messages.ChatInviteImporters;

chatAdminWithInvites#f2ecef23 admin_id:long invites_count:int revoked_invites_count:int = ChatAdminWithInvites;

messages.chatAdminsWithInvites#b69b72d7 admins:Vector<ChatAdminWithInvites> users:Vector<User> = messages.ChatAdminsWithInvites;

messages.checkedHistoryImportPeer#a24de717 confirm_text:string = messages.CheckedHistoryImportPeer;

phone.joinAsPeers#afe5623f peers:Vector<Peer> chats:Vector<Chat> users:Vector<User> = phone.JoinAsPeers;

phone.exportedGroupCallInvite#204bd158 link:string = phone.ExportedGroupCallInvite;

groupCallParticipantVideoSourceGroup#dcb118b7 semantics:string sources:Vector<int> = GroupCallParticipantVideoSourceGroup;

groupCallParticipantVideo#67753ac8 flags:# paused:flags.0?true endpoint:string source_groups:Vector<GroupCallParticipantVideoSourceGroup> audio_source:flags.1?int = GroupCallParticipantVideo;

stickers.suggestedShortName#85fea03f short_name:string = stickers.SuggestedShortName;

botCommandScopeDefault#2f6cb2ab = BotCommandScope;
botCommandScopeUsers#3c4f04d8 = BotCommandScope;
botCommandScopeChats#6fe1a881 = BotCommandScope;
botCommandScopeChatAdmins#b9aa606a = BotCommandScope;
botCommandScopePeer#db9d897d peer:InputPeer = BotCommandScope;
botCommandScopePeerAdmins#3fd863d1 peer:InputPeer = BotCommandScope;
botCommandScopePeerUser#a1321f3 peer:InputPeer user_id:InputUser = BotCommandScope;

account.resetPasswordFailedWait#e3779861 retry_date:int = account.ResetPasswordResult;
account.resetPasswordRequestedWait#e9effc7d until_date:int = account.ResetPasswordResult;
account.resetPasswordOk#e926d63e = account.ResetPasswordResult;

sponsoredMessage#3a836df8 flags:# recommended:flags.5?true random_id:bytes from_id:flags.3?Peer chat_invite:flags.4?ChatInvite chat_invite_hash:flags.4?string channel_post:flags.2?int start_param:flags.0?string message:string entities:flags.1?Vector<MessageEntity> = SponsoredMessage;

messages.sponsoredMessages#65a4c7d5 messages:Vector<SponsoredMessage> chats:Vector<Chat> users:Vector<User> = messages.SponsoredMessages;

searchResultsCalendarPeriod#c9b0539f date:int min_msg_id:int max_msg_id:int count:int = SearchResultsCalendarPeriod;

messages.searchResultsCalendar#147ee23c flags:# inexact:flags.0?true count:int min_date:int min_msg_id:int offset_id_offset:flags.1?int periods:Vector<SearchResultsCalendarPeriod> messages:Vector<Message> chats:Vector<Chat> users:Vector<User> = messages.SearchResultsCalendar;

searchResultPosition#7f648b67 msg_id:int date:int offset:int = SearchResultsPosition;

messages.searchResultsPositions#53b22baf count:int positions:Vector<SearchResultsPosition> = messages.SearchResultsPositions;

channels.sendAsPeers#f496b0c6 peers:Vector<SendAsPeer> chats:Vector<Chat> users:Vector<User> = channels.SendAsPeers;

users.userFull#3b6d152e full_user:UserFull chats:Vector<Chat> users:Vector<User> = users.UserFull;

messages.peerSettings#6880b94d settings:PeerSettings chats:Vector<Chat> users:Vector<User> = messages.PeerSettings;

auth.loggedOut#c3a2835f flags:# future_auth_token:flags.0?bytes = auth.LoggedOut;

reactionCount#a3d1cb80 flags:# chosen_order:flags.0?int reaction:Reaction count:int = ReactionCount;

messageReactions#4f2b9479 flags:# min:flags.0?true can_see_list:flags.2?true results:Vector<ReactionCount> recent_reactions:flags.1?Vector<MessagePeerReaction> = MessageReactions;

messages.messageReactionsList#31bd492d flags:# count:int reactions:Vector<MessagePeerReaction> chats:Vector<Chat> users:Vector<User> next_offset:flags.0?string = messages.MessageReactionsList;

availableReaction#c077ec01 flags:# inactive:flags.0?true premium:flags.2?true reaction:string title:string static_icon:Document appear_animation:Document select_animation:Document activate_animation:Document effect_animation:Document around_animation:flags.1?Document center_icon:flags.1?Document = AvailableReaction;

messages.availableReactionsNotModified#9f071957 = messages.AvailableReactions;
messages.availableReactions#768e3aad hash:int reactions:Vector<AvailableReaction> = messages.AvailableReactions;

messages.translateNoResult#67ca4737 = messages.TranslatedText;
messages.translateResultText#a214f7d0 text:string = messages.TranslatedText;

messagePeerReaction#b156fe9c flags:# big:flags.0?true unread:flags.1?true peer_id:Peer reaction:Reaction = MessagePeerReaction;

groupCallStreamChannel#80eb48af channel:int scale:int last_timestamp_ms:long = GroupCallStreamChannel;

phone.groupCallStreamChannels#d0e482b2 channels:Vector<GroupCallStreamChannel> = phone.GroupCallStreamChannels;

phone.groupCallStreamRtmpUrl#2dbf3432 url:string key:string = phone.GroupCallStreamRtmpUrl;

attachMenuBotIconColor#4576f3f0 name:string color:int = AttachMenuBotIconColor;

attachMenuBotIcon#b2a7386b flags:# name:string icon:Document colors:flags.0?Vector<AttachMenuBotIconColor> = AttachMenuBotIcon;

attachMenuBot#c8aa2cd2 flags:# inactive:flags.0?true has_settings:flags.1?true bot_id:long short_name:string peer_types:Vector<AttachMenuPeerType> icons:Vector<AttachMenuBotIcon> = AttachMenuBot;

attachMenuBotsNotModified#f1d88a5c = AttachMenuBots;
attachMenuBots#3c4301c0 hash:long bots:Vector<AttachMenuBot> users:Vector<User> = AttachMenuBots;

attachMenuBotsBot#93bf667f bot:AttachMenuBot users:Vector<User> = AttachMenuBotsBot;

webViewResultUrl#c14557c query_id:long url:string = WebViewResult;

simpleWebViewResultUrl#882f76bb url:string = SimpleWebViewResult;

webViewMessageSent#c94511c flags:# msg_id:flags.0?InputBotInlineMessageID = WebViewMessageSent;

botMenuButtonDefault#7533a588 = BotMenuButton;
botMenuButtonCommands#4258c205 = BotMenuButton;
botMenuButton#c7b57ce6 text:string url:string = BotMenuButton;

account.savedRingtonesNotModified#fbf6e8b1 = account.SavedRingtones;
account.savedRingtones#c1e92cc5 hash:long ringtones:Vector<Document> = account.SavedRingtones;

notificationSoundDefault#97e8bebe = NotificationSound;
notificationSoundNone#6f0c34df = NotificationSound;
notificationSoundLocal#830b9ae4 title:string data:string = NotificationSound;
notificationSoundRingtone#ff6c8049 id:long = NotificationSound;

account.savedRingtone#b7263f6d = account.SavedRingtone;
account.savedRingtoneConverted#1f307eb7 document:Document = account.SavedRingtone;

attachMenuPeerTypeSameBotPM#7d6be90e = AttachMenuPeerType;
attachMenuPeerTypeBotPM#c32bfa1a = AttachMenuPeerType;
attachMenuPeerTypePM#f146d31f = AttachMenuPeerType;
attachMenuPeerTypeChat#509113f = AttachMenuPeerType;
attachMenuPeerTypeBroadcast#7bfbdefc = AttachMenuPeerType;

inputInvoiceMessage#c5b56859 peer:InputPeer msg_id:int = InputInvoice;
inputInvoiceSlug#c326caef slug:string = InputInvoice;

payments.exportedInvoice#aed0cbd9 url:string = payments.ExportedInvoice;

messages.transcribedAudio#93752c52 flags:# pending:flags.0?true transcription_id:long text:string = messages.TranscribedAudio;

help.premiumPromo#5334759c status_text:string status_entities:Vector<MessageEntity> video_sections:Vector<string> videos:Vector<Document> period_options:Vector<PremiumSubscriptionOption> users:Vector<User> = help.PremiumPromo;

inputStorePaymentPremiumSubscription#a6751e66 flags:# restore:flags.0?true = InputStorePaymentPurpose;
inputStorePaymentGiftPremium#616f7fe8 user_id:InputUser currency:string amount:long = InputStorePaymentPurpose;

premiumGiftOption#74c34319 flags:# months:int currency:string amount:long bot_url:string store_product:flags.0?string = PremiumGiftOption;

paymentFormMethod#88f8f21b url:string title:string = PaymentFormMethod;

emojiStatusEmpty#2de11aae = EmojiStatus;
emojiStatus#929b619d document_id:long = EmojiStatus;
emojiStatusUntil#fa30a8c7 document_id:long until:int = EmojiStatus;

account.emojiStatusesNotModified#d08ce645 = account.EmojiStatuses;
account.emojiStatuses#90c467d1 hash:long statuses:Vector<EmojiStatus> = account.EmojiStatuses;

reactionEmpty#79f5d419 = Reaction;
reactionEmoji#1b2286b8 emoticon:string = Reaction;
reactionCustomEmoji#8935fc73 document_id:long = Reaction;

chatReactionsNone#eafc32bc = ChatReactions;
chatReactionsAll#52928bca flags:# allow_custom:flags.0?true = ChatReactions;
chatReactionsSome#661d4037 reactions:Vector<Reaction> = ChatReactions;

messages.reactionsNotModified#b06fdbdf = messages.Reactions;
messages.reactions#eafdf716 hash:long reactions:Vector<Reaction> = messages.Reactions;

emailVerifyPurposeLoginSetup#4345be73 phone_number:string phone_code_hash:string = EmailVerifyPurpose;
emailVerifyPurposeLoginChange#527d22eb = EmailVerifyPurpose;
emailVerifyPurposePassport#bbf51685 = EmailVerifyPurpose;

emailVerificationCode#922e55a9 code:string = EmailVerification;
emailVerificationGoogle#db909ec2 token:string = EmailVerification;
emailVerificationApple#96d074fd token:string = EmailVerification;

account.emailVerified#2b96cd1b email:string = account.EmailVerified;
account.emailVerifiedLogin#e1bb0d61 email:string sent_code:auth.SentCode = account.EmailVerified;

premiumSubscriptionOption#b6f11ebe flags:# current:flags.1?true can_purchase_upgrade:flags.2?true months:int currency:string amount:long bot_url:string store_product:flags.0?string = PremiumSubscriptionOption;

sendAsPeer#b81c7034 flags:# premium_required:flags.0?true peer:Peer = SendAsPeer;

messageExtendedMediaPreview#ad628cc8 flags:# w:flags.0?int h:flags.0?int thumb:flags.1?PhotoSize video_duration:flags.2?int = MessageExtendedMedia;
messageExtendedMedia#ee479c64 media:MessageMedia = MessageExtendedMedia;

---functions---

invokeAfterMsg#cb9f372d {X:Type} msg_id:long query:!X = X;
invokeAfterMsgs#3dc4b4f0 {X:Type} msg_ids:Vector<long> query:!X = X;
initConnection#c1cd5ea9 {X:Type} flags:# api_id:int device_model:string system_version:string app_version:string system_lang_code:string lang_pack:string lang_code:string proxy:flags.0?InputClientProxy params:flags.1?JSONValue query:!X = X;
invokeWithLayer#da9b0d0d {X:Type} layer:int query:!X = X;
invokeWithoutUpdates#bf9459b7 {X:Type} query:!X = X;
invokeWithMessagesRange#365275f2 {X:Type} range:MessageRange query:!X = X;
invokeWithTakeout#aca9fd2e {X:Type} takeout_id:long query:!X = X;

auth.sendCode#a677244f phone_number:string api_id:int api_hash:string settings:CodeSettings = auth.SentCode;
auth.signUp#80eee427 phone_number:string phone_code_hash:string first_name:string last_name:string = auth.Authorization;
auth.signIn#8d52a951 flags:# phone_number:string phone_code_hash:string phone_code:flags.0?string email_verification:flags.1?EmailVerification = auth.Authorization;
auth.logOut#3e72ba19 = auth.LoggedOut;
auth.resetAuthorizations#9fab0d1a = Bool;
auth.exportAuthorization#e5bfffcd dc_id:int = auth.ExportedAuthorization;
auth.importAuthorization#a57a7dad id:long bytes:bytes = auth.Authorization;
auth.bindTempAuthKey#cdd42a05 perm_auth_key_id:long nonce:long expires_at:int encrypted_message:bytes = Bool;
auth.importBotAuthorization#67a3ff2c flags:int api_id:int api_hash:string bot_auth_token:string = auth.Authorization;
auth.checkPassword#d18b4d16 password:InputCheckPasswordSRP = auth.Authorization;
auth.requestPasswordRecovery#d897bc66 = auth.PasswordRecovery;
auth.recoverPassword#37096c70 flags:# code:string new_settings:flags.0?account.PasswordInputSettings = auth.Authorization;
auth.resendCode#3ef1a9bf phone_number:string phone_code_hash:string = auth.SentCode;
auth.cancelCode#1f040578 phone_number:string phone_code_hash:string = Bool;
auth.dropTempAuthKeys#8e48a188 except_auth_keys:Vector<long> = Bool;
auth.exportLoginToken#b7e085fe api_id:int api_hash:string except_ids:Vector<long> = auth.LoginToken;
auth.importLoginToken#95ac5ce4 token:bytes = auth.LoginToken;
auth.acceptLoginToken#e894ad4d token:bytes = Authorization;
auth.checkRecoveryPassword#d36bf79 code:string = Bool;

account.registerDevice#ec86017a flags:# no_muted:flags.0?true token_type:int token:string app_sandbox:Bool secret:bytes other_uids:Vector<long> = Bool;
account.unregisterDevice#6a0d3206 token_type:int token:string other_uids:Vector<long> = Bool;
account.updateNotifySettings#84be5b93 peer:InputNotifyPeer settings:InputPeerNotifySettings = Bool;
account.getNotifySettings#12b3ad31 peer:InputNotifyPeer = PeerNotifySettings;
account.resetNotifySettings#db7e1747 = Bool;
account.updateProfile#78515775 flags:# first_name:flags.0?string last_name:flags.1?string about:flags.2?string = User;
account.updateStatus#6628562c offline:Bool = Bool;
account.getWallPapers#7967d36 hash:long = account.WallPapers;
account.reportPeer#c5ba3d86 peer:InputPeer reason:ReportReason message:string = Bool;
account.checkUsername#2714d86c username:string = Bool;
account.updateUsername#3e0bdd7c username:string = User;
account.getPrivacy#dadbc950 key:InputPrivacyKey = account.PrivacyRules;
account.setPrivacy#c9f81ce8 key:InputPrivacyKey rules:Vector<InputPrivacyRule> = account.PrivacyRules;
account.deleteAccount#a2c0cf74 flags:# reason:string password:flags.0?InputCheckPasswordSRP = Bool;
account.getAccountTTL#8fc711d = AccountDaysTTL;
account.setAccountTTL#2442485e ttl:AccountDaysTTL = Bool;
account.sendChangePhoneCode#82574ae5 phone_number:string settings:CodeSettings = auth.SentCode;
account.changePhone#70c32edb phone_number:string phone_code_hash:string phone_code:string = User;
account.updateDeviceLocked#38df3532 period:int = Bool;
account.getAuthorizations#e320c158 = account.Authorizations;
account.resetAuthorization#df77f3bc hash:long = Bool;
account.getPassword#548a30f5 = account.Password;
account.getPasswordSettings#9cd4eaf9 password:InputCheckPasswordSRP = account.PasswordSettings;
account.updatePasswordSettings#a59b102f password:InputCheckPasswordSRP new_settings:account.PasswordInputSettings = Bool;
account.sendConfirmPhoneCode#1b3faa88 hash:string settings:CodeSettings = auth.SentCode;
account.confirmPhone#5f2178c3 phone_code_hash:string phone_code:string = Bool;
account.getTmpPassword#449e0b51 password:InputCheckPasswordSRP period:int = account.TmpPassword;
account.getWebAuthorizations#182e6d6f = account.WebAuthorizations;
account.resetWebAuthorization#2d01b9ef hash:long = Bool;
account.resetWebAuthorizations#682d2594 = Bool;
account.getAllSecureValues#b288bc7d = Vector<SecureValue>;
account.getSecureValue#73665bc2 types:Vector<SecureValueType> = Vector<SecureValue>;
account.saveSecureValue#899fe31d value:InputSecureValue secure_secret_id:long = SecureValue;
account.deleteSecureValue#b880bc4b types:Vector<SecureValueType> = Bool;
account.getAuthorizationForm#a929597a bot_id:long scope:string public_key:string = account.AuthorizationForm;
account.acceptAuthorization#f3ed4c73 bot_id:long scope:string public_key:string value_hashes:Vector<SecureValueHash> credentials:SecureCredentialsEncrypted = Bool;
account.sendVerifyPhoneCode#a5a356f9 phone_number:string settings:CodeSettings = auth.SentCode;
account.verifyPhone#4dd3a7f6 phone_number:string phone_code_hash:string phone_code:string = Bool;
account.sendVerifyEmailCode#98e037bb purpose:EmailVerifyPurpose email:string = account.SentEmailCode;
account.verifyEmail#32da4cf purpose:EmailVerifyPurpose verification:EmailVerification = account.EmailVerified;
account.initTakeoutSession#8ef3eab0 flags:# contacts:flags.0?true message_users:flags.1?true message_chats:flags.2?true message_megagroups:flags.3?true message_channels:flags.4?true files:flags.5?true file_max_size:flags.5?long = account.Takeout;
account.finishTakeoutSession#1d2652ee flags:# success:flags.0?true = Bool;
account.confirmPasswordEmail#8fdf1920 code:string = Bool;
account.resendPasswordEmail#7a7f2a15 = Bool;
account.cancelPasswordEmail#c1cbd5b6 = Bool;
account.getContactSignUpNotification#9f07c728 = Bool;
account.setContactSignUpNotification#cff43f61 silent:Bool = Bool;
account.getNotifyExceptions#53577479 flags:# compare_sound:flags.1?true peer:flags.0?InputNotifyPeer = Updates;
account.getWallPaper#fc8ddbea wallpaper:InputWallPaper = WallPaper;
account.uploadWallPaper#dd853661 file:InputFile mime_type:string settings:WallPaperSettings = WallPaper;
account.saveWallPaper#6c5a5b37 wallpaper:InputWallPaper unsave:Bool settings:WallPaperSettings = Bool;
account.installWallPaper#feed5769 wallpaper:InputWallPaper settings:WallPaperSettings = Bool;
account.resetWallPapers#bb3b9804 = Bool;
account.getAutoDownloadSettings#56da0b3f = account.AutoDownloadSettings;
account.saveAutoDownloadSettings#76f36233 flags:# low:flags.0?true high:flags.1?true settings:AutoDownloadSettings = Bool;
account.uploadTheme#1c3db333 flags:# file:InputFile thumb:flags.0?InputFile file_name:string mime_type:string = Document;
account.createTheme#652e4400 flags:# slug:string title:string document:flags.2?InputDocument settings:flags.3?Vector<InputThemeSettings> = Theme;
account.updateTheme#2bf40ccc flags:# format:string theme:InputTheme slug:flags.0?string title:flags.1?string document:flags.2?InputDocument settings:flags.3?Vector<InputThemeSettings> = Theme;
account.saveTheme#f257106c theme:InputTheme unsave:Bool = Bool;
account.installTheme#c727bb3b flags:# dark:flags.0?true theme:flags.1?InputTheme format:flags.2?string base_theme:flags.3?BaseTheme = Bool;
account.getTheme#8d9d742b format:string theme:InputTheme document_id:long = Theme;
account.getThemes#7206e458 format:string hash:long = account.Themes;
account.setContentSettings#b574b16b flags:# sensitive_enabled:flags.0?true = Bool;
account.getContentSettings#8b9b4dae = account.ContentSettings;
account.getMultiWallPapers#65ad71dc wallpapers:Vector<InputWallPaper> = Vector<WallPaper>;
account.getGlobalPrivacySettings#eb2b4cf6 = GlobalPrivacySettings;
account.setGlobalPrivacySettings#1edaaac2 settings:GlobalPrivacySettings = GlobalPrivacySettings;
account.reportProfilePhoto#fa8cc6f5 peer:InputPeer photo_id:InputPhoto reason:ReportReason message:string = Bool;
account.resetPassword#9308ce1b = account.ResetPasswordResult;
account.declinePasswordReset#4c9409f6 = Bool;
account.getChatThemes#d638de89 hash:long = account.Themes;
account.setAuthorizationTTL#bf899aa0 authorization_ttl_days:int = Bool;
account.changeAuthorizationSettings#40f48462 flags:# hash:long encrypted_requests_disabled:flags.0?Bool call_requests_disabled:flags.1?Bool = Bool;
account.getSavedRingtones#e1902288 hash:long = account.SavedRingtones;
account.saveRingtone#3dea5b03 id:InputDocument unsave:Bool = account.SavedRingtone;
account.uploadRingtone#831a83a2 file:InputFile file_name:string mime_type:string = Document;
account.updateEmojiStatus#fbd3de6b emoji_status:EmojiStatus = Bool;
account.getDefaultEmojiStatuses#d6753386 hash:long = account.EmojiStatuses;
account.getRecentEmojiStatuses#f578105 hash:long = account.EmojiStatuses;
account.clearRecentEmojiStatuses#18201aae = Bool;

users.getUsers#d91a548 id:Vector<InputUser> = Vector<User>;
users.getFullUser#b60f5918 id:InputUser = users.UserFull;
users.setSecureValueErrors#90c894b5 id:InputUser errors:Vector<SecureValueError> = Bool;

contacts.getContactIDs#7adc669d hash:long = Vector<int>;
contacts.getStatuses#c4a353ee = Vector<ContactStatus>;
contacts.getContacts#5dd69e12 hash:long = contacts.Contacts;
contacts.importContacts#2c800be5 contacts:Vector<InputContact> = contacts.ImportedContacts;
contacts.deleteContacts#96a0e00 id:Vector<InputUser> = Updates;
contacts.deleteByPhones#1013fd9e phones:Vector<string> = Bool;
contacts.block#68cc1411 id:InputPeer = Bool;
contacts.unblock#bea65d50 id:InputPeer = Bool;
contacts.getBlocked#f57c350f offset:int limit:int = contacts.Blocked;
contacts.search#11f812d8 q:string limit:int = contacts.Found;
contacts.resolveUsername#f93ccba3 username:string = contacts.ResolvedPeer;
contacts.getTopPeers#973478b6 flags:# correspondents:flags.0?true bots_pm:flags.1?true bots_inline:flags.2?true phone_calls:flags.3?true forward_users:flags.4?true forward_chats:flags.5?true groups:flags.10?true channels:flags.15?true offset:int limit:int hash:long = contacts.TopPeers;
contacts.resetTopPeerRating#1ae373ac category:TopPeerCategory peer:InputPeer = Bool;
contacts.resetSaved#879537f1 = Bool;
contacts.getSaved#82f1e39f = Vector<SavedContact>;
contacts.toggleTopPeers#8514bdda enabled:Bool = Bool;
contacts.addContact#e8f463d0 flags:# add_phone_privacy_exception:flags.0?true id:InputUser first_name:string last_name:string phone:string = Updates;
contacts.acceptContact#f831a20f id:InputUser = Updates;
contacts.getLocated#d348bc44 flags:# background:flags.1?true geo_point:InputGeoPoint self_expires:flags.0?int = Updates;
contacts.blockFromReplies#29a8962c flags:# delete_message:flags.0?true delete_history:flags.1?true report_spam:flags.2?true msg_id:int = Updates;
contacts.resolvePhone#8af94344 phone:string = contacts.ResolvedPeer;

messages.getMessages#63c66506 id:Vector<InputMessage> = messages.Messages;
messages.getDialogs#a0f4cb4f flags:# exclude_pinned:flags.0?true folder_id:flags.1?int offset_date:int offset_id:int offset_peer:InputPeer limit:int hash:long = messages.Dialogs;
messages.getHistory#4423e6c5 peer:InputPeer offset_id:int offset_date:int add_offset:int limit:int max_id:int min_id:int hash:long = messages.Messages;
messages.search#a0fda762 flags:# peer:InputPeer q:string from_id:flags.0?InputPeer top_msg_id:flags.1?int filter:MessagesFilter min_date:int max_date:int offset_id:int add_offset:int limit:int max_id:int min_id:int hash:long = messages.Messages;
messages.readHistory#e306d3a peer:InputPeer max_id:int = messages.AffectedMessages;
messages.deleteHistory#b08f922a flags:# just_clear:flags.0?true revoke:flags.1?true peer:InputPeer max_id:int min_date:flags.2?int max_date:flags.3?int = messages.AffectedHistory;
messages.deleteMessages#e58e95d2 flags:# revoke:flags.0?true id:Vector<int> = messages.AffectedMessages;
messages.receivedMessages#5a954c0 max_id:int = Vector<ReceivedNotifyMessage>;
messages.setTyping#58943ee2 flags:# peer:InputPeer top_msg_id:flags.0?int action:SendMessageAction = Bool;
messages.sendMessage#d9d75a4 flags:# no_webpage:flags.1?true silent:flags.5?true background:flags.6?true clear_draft:flags.7?true noforwards:flags.14?true update_stickersets_order:flags.15?true peer:InputPeer reply_to_msg_id:flags.0?int message:string random_id:long reply_markup:flags.2?ReplyMarkup entities:flags.3?Vector<MessageEntity> schedule_date:flags.10?int send_as:flags.13?InputPeer = Updates;
messages.sendMedia#e25ff8e0 flags:# silent:flags.5?true background:flags.6?true clear_draft:flags.7?true noforwards:flags.14?true update_stickersets_order:flags.15?true peer:InputPeer reply_to_msg_id:flags.0?int media:InputMedia message:string random_id:long reply_markup:flags.2?ReplyMarkup entities:flags.3?Vector<MessageEntity> schedule_date:flags.10?int send_as:flags.13?InputPeer = Updates;
messages.forwardMessages#cc30290b flags:# silent:flags.5?true background:flags.6?true with_my_score:flags.8?true drop_author:flags.11?true drop_media_captions:flags.12?true noforwards:flags.14?true from_peer:InputPeer id:Vector<int> random_id:Vector<long> to_peer:InputPeer schedule_date:flags.10?int send_as:flags.13?InputPeer = Updates;
messages.reportSpam#cf1592db peer:InputPeer = Bool;
messages.getPeerSettings#efd9a6a2 peer:InputPeer = messages.PeerSettings;
messages.report#8953ab4e peer:InputPeer id:Vector<int> reason:ReportReason message:string = Bool;
messages.getChats#49e9528f id:Vector<long> = messages.Chats;
messages.getFullChat#aeb00b34 chat_id:long = messages.ChatFull;
messages.editChatTitle#73783ffd chat_id:long title:string = Updates;
messages.editChatPhoto#35ddd674 chat_id:long photo:InputChatPhoto = Updates;
messages.addChatUser#f24753e3 chat_id:long user_id:InputUser fwd_limit:int = Updates;
messages.deleteChatUser#a2185cab flags:# revoke_history:flags.0?true chat_id:long user_id:InputUser = Updates;
messages.createChat#9cb126e users:Vector<InputUser> title:string = Updates;
messages.getDhConfig#26cf8950 version:int random_length:int = messages.DhConfig;
messages.requestEncryption#f64daf43 user_id:InputUser random_id:int g_a:bytes = EncryptedChat;
messages.acceptEncryption#3dbc0415 peer:InputEncryptedChat g_b:bytes key_fingerprint:long = EncryptedChat;
messages.discardEncryption#f393aea0 flags:# delete_history:flags.0?true chat_id:int = Bool;
messages.setEncryptedTyping#791451ed peer:InputEncryptedChat typing:Bool = Bool;
messages.readEncryptedHistory#7f4b690a peer:InputEncryptedChat max_date:int = Bool;
messages.sendEncrypted#44fa7a15 flags:# silent:flags.0?true peer:InputEncryptedChat random_id:long data:bytes = messages.SentEncryptedMessage;
messages.sendEncryptedFile#5559481d flags:# silent:flags.0?true peer:InputEncryptedChat random_id:long data:bytes file:InputEncryptedFile = messages.SentEncryptedMessage;
messages.sendEncryptedService#32d439a4 peer:InputEncryptedChat random_id:long data:bytes = messages.SentEncryptedMessage;
messages.receivedQueue#55a5bb66 max_qts:int = Vector<long>;
messages.reportEncryptedSpam#4b0c8c0f peer:InputEncryptedChat = Bool;
messages.readMessageContents#36a73f77 id:Vector<int> = messages.AffectedMessages;
messages.getStickers#d5a5d3a1 emoticon:string hash:long = messages.Stickers;
messages.getAllStickers#b8a0a1a8 hash:long = messages.AllStickers;
messages.getWebPagePreview#8b68b0cc flags:# message:string entities:flags.3?Vector<MessageEntity> = MessageMedia;
messages.exportChatInvite#a02ce5d5 flags:# legacy_revoke_permanent:flags.2?true request_needed:flags.3?true peer:InputPeer expire_date:flags.0?int usage_limit:flags.1?int title:flags.4?string = ExportedChatInvite;
messages.checkChatInvite#3eadb1bb hash:string = ChatInvite;
messages.importChatInvite#6c50051c hash:string = Updates;
messages.getStickerSet#c8a0ec74 stickerset:InputStickerSet hash:int = messages.StickerSet;
messages.installStickerSet#c78fe460 stickerset:InputStickerSet archived:Bool = messages.StickerSetInstallResult;
messages.uninstallStickerSet#f96e55de stickerset:InputStickerSet = Bool;
messages.startBot#e6df7378 bot:InputUser peer:InputPeer random_id:long start_param:string = Updates;
messages.getMessagesViews#5784d3e1 peer:InputPeer id:Vector<int> increment:Bool = messages.MessageViews;
messages.editChatAdmin#a85bd1c2 chat_id:long user_id:InputUser is_admin:Bool = Bool;
messages.migrateChat#a2875319 chat_id:long = Updates;
messages.searchGlobal#4bc6589a flags:# folder_id:flags.0?int q:string filter:MessagesFilter min_date:int max_date:int offset_rate:int offset_peer:InputPeer offset_id:int limit:int = messages.Messages;
messages.reorderStickerSets#78337739 flags:# masks:flags.0?true emojis:flags.1?true order:Vector<long> = Bool;
messages.getDocumentByHash#b1f2061f sha256:bytes size:long mime_type:string = Document;
messages.getSavedGifs#5cf09635 hash:long = messages.SavedGifs;
messages.saveGif#327a30cb id:InputDocument unsave:Bool = Bool;
messages.getInlineBotResults#514e999d flags:# bot:InputUser peer:InputPeer geo_point:flags.0?InputGeoPoint query:string offset:string = messages.BotResults;
messages.setInlineBotResults#eb5ea206 flags:# gallery:flags.0?true private:flags.1?true query_id:long results:Vector<InputBotInlineResult> cache_time:int next_offset:flags.2?string switch_pm:flags.3?InlineBotSwitchPM = Bool;
messages.sendInlineBotResult#7aa11297 flags:# silent:flags.5?true background:flags.6?true clear_draft:flags.7?true hide_via:flags.11?true peer:InputPeer reply_to_msg_id:flags.0?int random_id:long query_id:long id:string schedule_date:flags.10?int send_as:flags.13?InputPeer = Updates;
messages.getMessageEditData#fda68d36 peer:InputPeer id:int = messages.MessageEditData;
messages.editMessage#48f71778 flags:# no_webpage:flags.1?true peer:InputPeer id:int message:flags.11?string media:flags.14?InputMedia reply_markup:flags.2?ReplyMarkup entities:flags.3?Vector<MessageEntity> schedule_date:flags.15?int = Updates;
messages.editInlineBotMessage#83557dba flags:# no_webpage:flags.1?true id:InputBotInlineMessageID message:flags.11?string media:flags.14?InputMedia reply_markup:flags.2?ReplyMarkup entities:flags.3?Vector<MessageEntity> = Bool;
messages.getBotCallbackAnswer#9342ca07 flags:# game:flags.1?true peer:InputPeer msg_id:int data:flags.0?bytes password:flags.2?InputCheckPasswordSRP = messages.BotCallbackAnswer;
messages.setBotCallbackAnswer#d58f130a flags:# alert:flags.1?true query_id:long message:flags.0?string url:flags.2?string cache_time:int = Bool;
messages.getPeerDialogs#e470bcfd peers:Vector<InputDialogPeer> = messages.PeerDialogs;
messages.saveDraft#bc39e14b flags:# no_webpage:flags.1?true reply_to_msg_id:flags.0?int peer:InputPeer message:string entities:flags.3?Vector<MessageEntity> = Bool;
messages.getAllDrafts#6a3f8d65 = Updates;
messages.getFeaturedStickers#64780b14 hash:long = messages.FeaturedStickers;
messages.readFeaturedStickers#5b118126 id:Vector<long> = Bool;
messages.getRecentStickers#9da9403b flags:# attached:flags.0?true hash:long = messages.RecentStickers;
messages.saveRecentSticker#392718f8 flags:# attached:flags.0?true id:InputDocument unsave:Bool = Bool;
messages.clearRecentStickers#8999602d flags:# attached:flags.0?true = Bool;
messages.getArchivedStickers#57f17692 flags:# masks:flags.0?true emojis:flags.1?true offset_id:long limit:int = messages.ArchivedStickers;
messages.getMaskStickers#640f82b8 hash:long = messages.AllStickers;
messages.getAttachedStickers#cc5b67cc media:InputStickeredMedia = Vector<StickerSetCovered>;
messages.setGameScore#8ef8ecc0 flags:# edit_message:flags.0?true force:flags.1?true peer:InputPeer id:int user_id:InputUser score:int = Updates;
messages.setInlineGameScore#15ad9f64 flags:# edit_message:flags.0?true force:flags.1?true id:InputBotInlineMessageID user_id:InputUser score:int = Bool;
messages.getGameHighScores#e822649d peer:InputPeer id:int user_id:InputUser = messages.HighScores;
messages.getInlineGameHighScores#f635e1b id:InputBotInlineMessageID user_id:InputUser = messages.HighScores;
messages.getCommonChats#e40ca104 user_id:InputUser max_id:long limit:int = messages.Chats;
messages.getAllChats#875f74be except_ids:Vector<long> = messages.Chats;
messages.getWebPage#32ca8f91 url:string hash:int = WebPage;
messages.toggleDialogPin#a731e257 flags:# pinned:flags.0?true peer:InputDialogPeer = Bool;
messages.reorderPinnedDialogs#3b1adf37 flags:# force:flags.0?true folder_id:int order:Vector<InputDialogPeer> = Bool;
messages.getPinnedDialogs#d6b94df2 folder_id:int = messages.PeerDialogs;
messages.setBotShippingResults#e5f672fa flags:# query_id:long error:flags.0?string shipping_options:flags.1?Vector<ShippingOption> = Bool;
messages.setBotPrecheckoutResults#9c2dd95 flags:# success:flags.1?true query_id:long error:flags.0?string = Bool;
messages.uploadMedia#519bc2b1 peer:InputPeer media:InputMedia = MessageMedia;
messages.sendScreenshotNotification#c97df020 peer:InputPeer reply_to_msg_id:int random_id:long = Updates;
messages.getFavedStickers#4f1aaa9 hash:long = messages.FavedStickers;
messages.faveSticker#b9ffc55b id:InputDocument unfave:Bool = Bool;
messages.getUnreadMentions#46578472 peer:InputPeer offset_id:int add_offset:int limit:int max_id:int min_id:int = messages.Messages;
messages.readMentions#f0189d3 peer:InputPeer = messages.AffectedHistory;
messages.getRecentLocations#702a40e0 peer:InputPeer limit:int hash:long = messages.Messages;
messages.sendMultiMedia#f803138f flags:# silent:flags.5?true background:flags.6?true clear_draft:flags.7?true noforwards:flags.14?true update_stickersets_order:flags.15?true peer:InputPeer reply_to_msg_id:flags.0?int multi_media:Vector<InputSingleMedia> schedule_date:flags.10?int send_as:flags.13?InputPeer = Updates;
messages.uploadEncryptedFile#5057c497 peer:InputEncryptedChat file:InputEncryptedFile = EncryptedFile;
messages.searchStickerSets#35705b8a flags:# exclude_featured:flags.0?true q:string hash:long = messages.FoundStickerSets;
messages.getSplitRanges#1cff7e08 = Vector<MessageRange>;
messages.markDialogUnread#c286d98f flags:# unread:flags.0?true peer:InputDialogPeer = Bool;
messages.getDialogUnreadMarks#22e24e22 = Vector<DialogPeer>;
messages.clearAllDrafts#7e58ee9c = Bool;
messages.updatePinnedMessage#d2aaf7ec flags:# silent:flags.0?true unpin:flags.1?true pm_oneside:flags.2?true peer:InputPeer id:int = Updates;
messages.sendVote#10ea6184 peer:InputPeer msg_id:int options:Vector<bytes> = Updates;
messages.getPollResults#73bb643b peer:InputPeer msg_id:int = Updates;
messages.getOnlines#6e2be050 peer:InputPeer = ChatOnlines;
messages.editChatAbout#def60797 peer:InputPeer about:string = Bool;
messages.editChatDefaultBannedRights#a5866b41 peer:InputPeer banned_rights:ChatBannedRights = Updates;
messages.getEmojiKeywords#35a0e062 lang_code:string = EmojiKeywordsDifference;
messages.getEmojiKeywordsDifference#1508b6af lang_code:string from_version:int = EmojiKeywordsDifference;
messages.getEmojiKeywordsLanguages#4e9963b2 lang_codes:Vector<string> = Vector<EmojiLanguage>;
messages.getEmojiURL#d5b10c26 lang_code:string = EmojiURL;
messages.getSearchCounters#732eef00 peer:InputPeer filters:Vector<MessagesFilter> = Vector<messages.SearchCounter>;
messages.requestUrlAuth#198fb446 flags:# peer:flags.1?InputPeer msg_id:flags.1?int button_id:flags.1?int url:flags.2?string = UrlAuthResult;
messages.acceptUrlAuth#b12c7125 flags:# write_allowed:flags.0?true peer:flags.1?InputPeer msg_id:flags.1?int button_id:flags.1?int url:flags.2?string = UrlAuthResult;
messages.hidePeerSettingsBar#4facb138 peer:InputPeer = Bool;
messages.getScheduledHistory#f516760b peer:InputPeer hash:long = messages.Messages;
messages.getScheduledMessages#bdbb0464 peer:InputPeer id:Vector<int> = messages.Messages;
messages.sendScheduledMessages#bd38850a peer:InputPeer id:Vector<int> = Updates;
messages.deleteScheduledMessages#59ae2b16 peer:InputPeer id:Vector<int> = Updates;
messages.getPollVotes#b86e380e flags:# peer:InputPeer id:int option:flags.0?bytes offset:flags.1?string limit:int = messages.VotesList;
messages.toggleStickerSets#b5052fea flags:# uninstall:flags.0?true archive:flags.1?true unarchive:flags.2?true stickersets:Vector<InputStickerSet> = Bool;
messages.getDialogFilters#f19ed96d = Vector<DialogFilter>;
messages.getSuggestedDialogFilters#a29cd42c = Vector<DialogFilterSuggested>;
messages.updateDialogFilter#1ad4a04a flags:# id:int filter:flags.0?DialogFilter = Bool;
messages.updateDialogFiltersOrder#c563c1e4 order:Vector<int> = Bool;
messages.getOldFeaturedStickers#7ed094a1 offset:int limit:int hash:long = messages.FeaturedStickers;
messages.getReplies#22ddd30c peer:InputPeer msg_id:int offset_id:int offset_date:int add_offset:int limit:int max_id:int min_id:int hash:long = messages.Messages;
messages.getDiscussionMessage#446972fd peer:InputPeer msg_id:int = messages.DiscussionMessage;
messages.readDiscussion#f731a9f4 peer:InputPeer msg_id:int read_max_id:int = Bool;
messages.unpinAllMessages#f025bc8b peer:InputPeer = messages.AffectedHistory;
messages.deleteChat#5bd0ee50 chat_id:long = Bool;
messages.deletePhoneCallHistory#f9cbe409 flags:# revoke:flags.0?true = messages.AffectedFoundMessages;
messages.checkHistoryImport#43fe19f3 import_head:string = messages.HistoryImportParsed;
messages.initHistoryImport#34090c3b peer:InputPeer file:InputFile media_count:int = messages.HistoryImport;
messages.uploadImportedMedia#2a862092 peer:InputPeer import_id:long file_name:string media:InputMedia = MessageMedia;
messages.startHistoryImport#b43df344 peer:InputPeer import_id:long = Bool;
messages.getExportedChatInvites#a2b5a3f6 flags:# revoked:flags.3?true peer:InputPeer admin_id:InputUser offset_date:flags.2?int offset_link:flags.2?string limit:int = messages.ExportedChatInvites;
messages.getExportedChatInvite#73746f5c peer:InputPeer link:string = messages.ExportedChatInvite;
messages.editExportedChatInvite#bdca2f75 flags:# revoked:flags.2?true peer:InputPeer link:string expire_date:flags.0?int usage_limit:flags.1?int request_needed:flags.3?Bool title:flags.4?string = messages.ExportedChatInvite;
messages.deleteRevokedExportedChatInvites#56987bd5 peer:InputPeer admin_id:InputUser = Bool;
messages.deleteExportedChatInvite#d464a42b peer:InputPeer link:string = Bool;
messages.getAdminsWithInvites#3920e6ef peer:InputPeer = messages.ChatAdminsWithInvites;
messages.getChatInviteImporters#df04dd4e flags:# requested:flags.0?true peer:InputPeer link:flags.1?string q:flags.2?string offset_date:int offset_user:InputUser limit:int = messages.ChatInviteImporters;
messages.setHistoryTTL#b80e5fe4 peer:InputPeer period:int = Updates;
messages.checkHistoryImportPeer#5dc60f03 peer:InputPeer = messages.CheckedHistoryImportPeer;
messages.setChatTheme#e63be13f peer:InputPeer emoticon:string = Updates;
messages.getMessageReadParticipants#2c6f97b7 peer:InputPeer msg_id:int = Vector<long>;
messages.getSearchResultsCalendar#49f0bde9 peer:InputPeer filter:MessagesFilter offset_id:int offset_date:int = messages.SearchResultsCalendar;
messages.getSearchResultsPositions#6e9583a3 peer:InputPeer filter:MessagesFilter offset_id:int limit:int = messages.SearchResultsPositions;
messages.hideChatJoinRequest#7fe7e815 flags:# approved:flags.0?true peer:InputPeer user_id:InputUser = Updates;
messages.hideAllChatJoinRequests#e085f4ea flags:# approved:flags.0?true peer:InputPeer link:flags.1?string = Updates;
messages.toggleNoForwards#b11eafa2 peer:InputPeer enabled:Bool = Updates;
messages.saveDefaultSendAs#ccfddf96 peer:InputPeer send_as:InputPeer = Bool;
messages.sendReaction#d30d78d4 flags:# big:flags.1?true add_to_recent:flags.2?true peer:InputPeer msg_id:int reaction:flags.0?Vector<Reaction> = Updates;
messages.getMessagesReactions#8bba90e6 peer:InputPeer id:Vector<int> = Updates;
messages.getMessageReactionsList#461b3f48 flags:# peer:InputPeer id:int reaction:flags.0?Reaction offset:flags.1?string limit:int = messages.MessageReactionsList;
messages.setChatAvailableReactions#feb16771 peer:InputPeer available_reactions:ChatReactions = Updates;
messages.getAvailableReactions#18dea0ac hash:int = messages.AvailableReactions;
messages.setDefaultReaction#4f47a016 reaction:Reaction = Bool;
messages.translateText#24ce6dee flags:# peer:flags.0?InputPeer msg_id:flags.0?int text:flags.1?string from_lang:flags.2?string to_lang:string = messages.TranslatedText;
messages.getUnreadReactions#e85bae1a peer:InputPeer offset_id:int add_offset:int limit:int max_id:int min_id:int = messages.Messages;
messages.readReactions#82e251d7 peer:InputPeer = messages.AffectedHistory;
messages.searchSentMedia#107e31a0 q:string filter:MessagesFilter limit:int = messages.Messages;
messages.getAttachMenuBots#16fcc2cb hash:long = AttachMenuBots;
messages.getAttachMenuBot#77216192 bot:InputUser = AttachMenuBotsBot;
messages.toggleBotInAttachMenu#1aee33af bot:InputUser enabled:Bool = Bool;
messages.requestWebView#fc87a53c flags:# from_bot_menu:flags.4?true silent:flags.5?true peer:InputPeer bot:InputUser url:flags.1?string start_param:flags.3?string theme_params:flags.2?DataJSON platform:string reply_to_msg_id:flags.0?int send_as:flags.13?InputPeer = WebViewResult;
messages.prolongWebView#ea5fbcce flags:# silent:flags.5?true peer:InputPeer bot:InputUser query_id:long reply_to_msg_id:flags.0?int send_as:flags.13?InputPeer = Bool;
messages.requestSimpleWebView#299bec8e flags:# bot:InputUser url:string theme_params:flags.0?DataJSON platform:string = SimpleWebViewResult;
messages.sendWebViewResultMessage#a4314f5 bot_query_id:string result:InputBotInlineResult = WebViewMessageSent;
messages.sendWebViewData#dc0242c8 bot:InputUser random_id:long button_text:string data:string = Updates;
messages.transcribeAudio#269e9a49 peer:InputPeer msg_id:int = messages.TranscribedAudio;
messages.rateTranscribedAudio#7f1d072f peer:InputPeer msg_id:int transcription_id:long good:Bool = Bool;
messages.getCustomEmojiDocuments#d9ab0f54 document_id:Vector<long> = Vector<Document>;
messages.getEmojiStickers#fbfca18f hash:long = messages.AllStickers;
messages.getFeaturedEmojiStickers#ecf6736 hash:long = messages.FeaturedStickers;
messages.reportReaction#3f64c076 peer:InputPeer id:int reaction_peer:InputPeer = Bool;
messages.getTopReactions#bb8125ba limit:int hash:long = messages.Reactions;
messages.getRecentReactions#39461db2 limit:int hash:long = messages.Reactions;
messages.clearRecentReactions#9dfeefb4 = Bool;
messages.getExtendedMedia#84f80814 peer:InputPeer id:Vector<int> = Updates;

updates.getState#edd4882a = updates.State;
updates.getDifference#25939651 flags:# pts:int pts_total_limit:flags.0?int date:int qts:int = updates.Difference;
updates.getChannelDifference#3173d78 flags:# force:flags.0?true channel:InputChannel filter:ChannelMessagesFilter pts:int limit:int = updates.ChannelDifference;

photos.updateProfilePhoto#72d4742c id:InputPhoto = photos.Photo;
photos.uploadProfilePhoto#89f30f69 flags:# file:flags.0?InputFile video:flags.1?InputFile video_start_ts:flags.2?double = photos.Photo;
photos.deletePhotos#87cf7f2f id:Vector<InputPhoto> = Vector<long>;
photos.getUserPhotos#91cd32a8 user_id:InputUser offset:int max_id:long limit:int = photos.Photos;

upload.saveFilePart#b304a621 file_id:long file_part:int bytes:bytes = Bool;
upload.getFile#be5335be flags:# precise:flags.0?true cdn_supported:flags.1?true location:InputFileLocation offset:long limit:int = upload.File;
upload.saveBigFilePart#de7b673d file_id:long file_part:int file_total_parts:int bytes:bytes = Bool;
upload.getWebFile#24e6818d location:InputWebFileLocation offset:int limit:int = upload.WebFile;
upload.getCdnFile#395f69da file_token:bytes offset:long limit:int = upload.CdnFile;
upload.reuploadCdnFile#9b2754a8 file_token:bytes request_token:bytes = Vector<FileHash>;
upload.getCdnFileHashes#91dc3f31 file_token:bytes offset:long = Vector<FileHash>;
upload.getFileHashes#9156982a location:InputFileLocation offset:long = Vector<FileHash>;

help.getConfig#c4f9186b = Config;
help.getNearestDc#1fb33026 = NearestDc;
help.getAppUpdate#522d5a7d source:string = help.AppUpdate;
help.getInviteText#4d392343 = help.InviteText;
help.getSupport#9cdf08cd = help.Support;
help.getAppChangelog#9010ef6f prev_app_version:string = Updates;
help.setBotUpdatesStatus#ec22cfcd pending_updates_count:int message:string = Bool;
help.getCdnConfig#52029342 = CdnConfig;
help.getRecentMeUrls#3dc0f114 referer:string = help.RecentMeUrls;
help.getTermsOfServiceUpdate#2ca51fd1 = help.TermsOfServiceUpdate;
help.acceptTermsOfService#ee72f79a id:DataJSON = Bool;
help.getDeepLinkInfo#3fedc75f path:string = help.DeepLinkInfo;
help.getAppConfig#98914110 = JSONValue;
help.saveAppLog#6f02f748 events:Vector<InputAppEvent> = Bool;
help.getPassportConfig#c661ad08 hash:int = help.PassportConfig;
help.getSupportName#d360e72c = help.SupportName;
help.getUserInfo#38a08d3 user_id:InputUser = help.UserInfo;
help.editUserInfo#66b91b70 user_id:InputUser message:string entities:Vector<MessageEntity> = help.UserInfo;
help.getPromoData#c0977421 = help.PromoData;
help.hidePromoData#1e251c95 peer:InputPeer = Bool;
help.dismissSuggestion#f50dbaa1 peer:InputPeer suggestion:string = Bool;
help.getCountriesList#735787a8 lang_code:string hash:int = help.CountriesList;
help.getPremiumPromo#b81b93d4 = help.PremiumPromo;

channels.readHistory#cc104937 channel:InputChannel max_id:int = Bool;
channels.deleteMessages#84c1fd4e channel:InputChannel id:Vector<int> = messages.AffectedMessages;
channels.reportSpam#f44a8315 channel:InputChannel participant:InputPeer id:Vector<int> = Bool;
channels.getMessages#ad8c9a23 channel:InputChannel id:Vector<InputMessage> = messages.Messages;
channels.getParticipants#77ced9d0 channel:InputChannel filter:ChannelParticipantsFilter offset:int limit:int hash:long = channels.ChannelParticipants;
channels.getParticipant#a0ab6cc6 channel:InputChannel participant:InputPeer = channels.ChannelParticipant;
channels.getChannels#a7f6bbb id:Vector<InputChannel> = messages.Chats;
channels.getFullChannel#8736a09 channel:InputChannel = messages.ChatFull;
channels.createChannel#3d5fb10f flags:# broadcast:flags.0?true megagroup:flags.1?true for_import:flags.3?true title:string about:string geo_point:flags.2?InputGeoPoint address:flags.2?string = Updates;
channels.editAdmin#d33c8902 channel:InputChannel user_id:InputUser admin_rights:ChatAdminRights rank:string = Updates;
channels.editTitle#566decd0 channel:InputChannel title:string = Updates;
channels.editPhoto#f12e57c9 channel:InputChannel photo:InputChatPhoto = Updates;
channels.checkUsername#10e6bd2c channel:InputChannel username:string = Bool;
channels.updateUsername#3514b3de channel:InputChannel username:string = Bool;
channels.joinChannel#24b524c5 channel:InputChannel = Updates;
channels.leaveChannel#f836aa95 channel:InputChannel = Updates;
channels.inviteToChannel#199f3a6c channel:InputChannel users:Vector<InputUser> = Updates;
channels.deleteChannel#c0111fe3 channel:InputChannel = Updates;
channels.exportMessageLink#e63fadeb flags:# grouped:flags.0?true thread:flags.1?true channel:InputChannel id:int = ExportedMessageLink;
channels.toggleSignatures#1f69b606 channel:InputChannel enabled:Bool = Updates;
channels.getAdminedPublicChannels#f8b036af flags:# by_location:flags.0?true check_limit:flags.1?true = messages.Chats;
channels.editBanned#96e6cd81 channel:InputChannel participant:InputPeer banned_rights:ChatBannedRights = Updates;
channels.getAdminLog#33ddf480 flags:# channel:InputChannel q:string events_filter:flags.0?ChannelAdminLogEventsFilter admins:flags.1?Vector<InputUser> max_id:long min_id:long limit:int = channels.AdminLogResults;
channels.setStickers#ea8ca4f9 channel:InputChannel stickerset:InputStickerSet = Bool;
channels.readMessageContents#eab5dc38 channel:InputChannel id:Vector<int> = Bool;
channels.deleteHistory#9baa9647 flags:# for_everyone:flags.0?true channel:InputChannel max_id:int = Updates;
channels.togglePreHistoryHidden#eabbb94c channel:InputChannel enabled:Bool = Updates;
channels.getLeftChannels#8341ecc0 offset:int = messages.Chats;
channels.getGroupsForDiscussion#f5dad378 = messages.Chats;
channels.setDiscussionGroup#40582bb2 broadcast:InputChannel group:InputChannel = Bool;
channels.editCreator#8f38cd1f channel:InputChannel user_id:InputUser password:InputCheckPasswordSRP = Updates;
channels.editLocation#58e63f6d channel:InputChannel geo_point:InputGeoPoint address:string = Bool;
channels.toggleSlowMode#edd49ef0 channel:InputChannel seconds:int = Updates;
channels.getInactiveChannels#11e831ee = messages.InactiveChats;
channels.convertToGigagroup#b290c69 channel:InputChannel = Updates;
channels.viewSponsoredMessage#beaedb94 channel:InputChannel random_id:bytes = Bool;
channels.getSponsoredMessages#ec210fbf channel:InputChannel = messages.SponsoredMessages;
channels.getSendAs#dc770ee peer:InputPeer = channels.SendAsPeers;
channels.deleteParticipantHistory#367544db channel:InputChannel participant:InputPeer = messages.AffectedHistory;
channels.toggleJoinToSend#e4cb9580 channel:InputChannel enabled:Bool = Updates;
channels.toggleJoinRequest#4c2985b6 channel:InputChannel enabled:Bool = Updates;

bots.sendCustomRequest#aa2769ed custom_method:string params:DataJSON = DataJSON;
bots.answerWebhookJSONQuery#e6213f4d query_id:long data:DataJSON = Bool;
bots.setBotCommands#517165a scope:BotCommandScope lang_code:string commands:Vector<BotCommand> = Bool;
bots.resetBotCommands#3d8de0f9 scope:BotCommandScope lang_code:string = Bool;
bots.getBotCommands#e34c0dd6 scope:BotCommandScope lang_code:string = Vector<BotCommand>;
bots.setBotMenuButton#4504d54f user_id:InputUser button:BotMenuButton = Bool;
bots.getBotMenuButton#9c60eb28 user_id:InputUser = BotMenuButton;
bots.setBotBroadcastDefaultAdminRights#788464e1 admin_rights:ChatAdminRights = Bool;
bots.setBotGroupDefaultAdminRights#925ec9ea admin_rights:ChatAdminRights = Bool;

payments.getPaymentForm#37148dbb flags:# invoice:InputInvoice theme_params:flags.0?DataJSON = payments.PaymentForm;
payments.getPaymentReceipt#2478d1cc peer:InputPeer msg_id:int = payments.PaymentReceipt;
payments.validateRequestedInfo#b6c8f12b flags:# save:flags.0?true invoice:InputInvoice info:PaymentRequestedInfo = payments.ValidatedRequestedInfo;
payments.sendPaymentForm#2d03522f flags:# form_id:long invoice:InputInvoice requested_info_id:flags.0?string shipping_option_id:flags.1?string credentials:InputPaymentCredentials tip_amount:flags.2?long = payments.PaymentResult;
payments.getSavedInfo#227d824b = payments.SavedInfo;
payments.clearSavedInfo#d83d70c1 flags:# credentials:flags.0?true info:flags.1?true = Bool;
payments.getBankCardData#2e79d779 number:string = payments.BankCardData;
payments.exportInvoice#f91b065 invoice_media:InputMedia = payments.ExportedInvoice;
payments.assignAppStoreTransaction#80ed747d receipt:bytes purpose:InputStorePaymentPurpose = Updates;
payments.assignPlayMarketTransaction#dffd50d3 receipt:DataJSON purpose:InputStorePaymentPurpose = Updates;
payments.canPurchasePremium#9fc19eb6 purpose:InputStorePaymentPurpose = Bool;

stickers.createStickerSet#9021ab67 flags:# masks:flags.0?true animated:flags.1?true videos:flags.4?true user_id:InputUser title:string short_name:string thumb:flags.2?InputDocument stickers:Vector<InputStickerSetItem> software:flags.3?string = messages.StickerSet;
stickers.removeStickerFromSet#f7760f51 sticker:InputDocument = messages.StickerSet;
stickers.changeStickerPosition#ffb6d4ca sticker:InputDocument position:int = messages.StickerSet;
stickers.addStickerToSet#8653febe stickerset:InputStickerSet sticker:InputStickerSetItem = messages.StickerSet;
stickers.setStickerSetThumb#9a364e30 stickerset:InputStickerSet thumb:InputDocument = messages.StickerSet;
stickers.checkShortName#284b3639 short_name:string = Bool;
stickers.suggestShortName#4dafc503 title:string = stickers.SuggestedShortName;

phone.getCallConfig#55451fa9 = DataJSON;
phone.requestCall#42ff96ed flags:# video:flags.0?true user_id:InputUser random_id:int g_a_hash:bytes protocol:PhoneCallProtocol = phone.PhoneCall;
phone.acceptCall#3bd2b4a0 peer:InputPhoneCall g_b:bytes protocol:PhoneCallProtocol = phone.PhoneCall;
phone.confirmCall#2efe1722 peer:InputPhoneCall g_a:bytes key_fingerprint:long protocol:PhoneCallProtocol = phone.PhoneCall;
phone.receivedCall#17d54f61 peer:InputPhoneCall = Bool;
phone.discardCall#b2cbc1c0 flags:# video:flags.0?true peer:InputPhoneCall duration:int reason:PhoneCallDiscardReason connection_id:long = Updates;
phone.setCallRating#59ead627 flags:# user_initiative:flags.0?true peer:InputPhoneCall rating:int comment:string = Updates;
phone.saveCallDebug#277add7e peer:InputPhoneCall debug:DataJSON = Bool;
phone.sendSignalingData#ff7a9383 peer:InputPhoneCall data:bytes = Bool;
phone.createGroupCall#48cdc6d8 flags:# rtmp_stream:flags.2?true peer:InputPeer random_id:int title:flags.0?string schedule_date:flags.1?int = Updates;
phone.joinGroupCall#b132ff7b flags:# muted:flags.0?true video_stopped:flags.2?true call:InputGroupCall join_as:InputPeer invite_hash:flags.1?string params:DataJSON = Updates;
phone.leaveGroupCall#500377f9 call:InputGroupCall source:int = Updates;
phone.inviteToGroupCall#7b393160 call:InputGroupCall users:Vector<InputUser> = Updates;
phone.discardGroupCall#7a777135 call:InputGroupCall = Updates;
phone.toggleGroupCallSettings#74bbb43d flags:# reset_invite_hash:flags.1?true call:InputGroupCall join_muted:flags.0?Bool = Updates;
phone.getGroupCall#41845db call:InputGroupCall limit:int = phone.GroupCall;
phone.getGroupParticipants#c558d8ab call:InputGroupCall ids:Vector<InputPeer> sources:Vector<int> offset:string limit:int = phone.GroupParticipants;
phone.checkGroupCall#b59cf977 call:InputGroupCall sources:Vector<int> = Vector<int>;
phone.toggleGroupCallRecord#f128c708 flags:# start:flags.0?true video:flags.2?true call:InputGroupCall title:flags.1?string video_portrait:flags.2?Bool = Updates;
phone.editGroupCallParticipant#a5273abf flags:# call:InputGroupCall participant:InputPeer muted:flags.0?Bool volume:flags.1?int raise_hand:flags.2?Bool video_stopped:flags.3?Bool video_paused:flags.4?Bool presentation_paused:flags.5?Bool = Updates;
phone.editGroupCallTitle#1ca6ac0a call:InputGroupCall title:string = Updates;
phone.getGroupCallJoinAs#ef7c213a peer:InputPeer = phone.JoinAsPeers;
phone.exportGroupCallInvite#e6aa647f flags:# can_self_unmute:flags.0?true call:InputGroupCall = phone.ExportedGroupCallInvite;
phone.toggleGroupCallStartSubscription#219c34e6 call:InputGroupCall subscribed:Bool = Updates;
phone.startScheduledGroupCall#5680e342 call:InputGroupCall = Updates;
phone.saveDefaultGroupCallJoinAs#575e1f8c peer:InputPeer join_as:InputPeer = Bool;
phone.joinGroupCallPresentation#cbea6bc4 call:InputGroupCall params:DataJSON = Updates;
phone.leaveGroupCallPresentation#1c50d144 call:InputGroupCall = Updates;
phone.getGroupCallStreamChannels#1ab21940 call:InputGroupCall = phone.GroupCallStreamChannels;
phone.getGroupCallStreamRtmpUrl#deb3abbf peer:InputPeer revoke:Bool = phone.GroupCallStreamRtmpUrl;
phone.saveCallLog#41248786 peer:InputPhoneCall file:InputFile = Bool;

langpack.getLangPack#f2f2330a lang_pack:string lang_code:string = LangPackDifference;
langpack.getStrings#efea3803 lang_pack:string lang_code:string keys:Vector<string> = Vector<LangPackString>;
langpack.getDifference#cd984aa5 lang_pack:string lang_code:string from_version:int = LangPackDifference;
langpack.getLanguages#42c6978f lang_pack:string = Vector<LangPackLanguage>;
langpack.getLanguage#6a596502 lang_pack:string lang_code:string = LangPackLanguage;

folders.editPeerFolders#6847d0ab folder_peers:Vector<InputFolderPeer> = Updates;
folders.deleteFolder#1c295881 folder_id:int = Updates;

stats.getBroadcastStats#ab42441a flags:# dark:flags.0?true channel:InputChannel = stats.BroadcastStats;
stats.loadAsyncGraph#621d5fa0 flags:# token:string x:flags.0?long = StatsGraph;
stats.getMegagroupStats#dcdf8607 flags:# dark:flags.0?true channel:InputChannel = stats.MegagroupStats;
stats.getMessagePublicForwards#5630281b channel:InputChannel msg_id:int offset_rate:int offset_peer:InputPeer offset_id:int limit:int = messages.Messages;
stats.getMessageStats#b6e0a3f5 flags:# dark:flags.0?true channel:InputChannel msg_id:int = stats.MessageStats;
*/
// LAYER 146
";
}