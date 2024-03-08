// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;

namespace Econolite.Ode.Api.JpoOdeTim;

public sealed record JpoOdeRsu(
    [property: JsonPropertyName("rsuIndex")]
    int RsuIndex,
    [property: JsonPropertyName("rsuTarget")]
    string RsuTarget,
    [property: JsonPropertyName("rsuUsername"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? RsuUsername,
    [property: JsonPropertyName("rsuPassword"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? RsuPassword,
    [property: JsonPropertyName("rsuRetries")]
    int RsuRetries,
    [property: JsonPropertyName("rsuTimeout")]
    int RsuTimeout
);

public sealed record JpoOdeSnmp(
    [property: JsonPropertyName("rsuid")] string RsuId,
    [property: JsonPropertyName("msgid")] string MsgId,
    [property: JsonPropertyName("mode")] string Mode,
    [property: JsonPropertyName("channel")]
    string Channel,
    [property: JsonPropertyName("interval")]
    string Interval,
    [property: JsonPropertyName("deliverystart")]
    string DeliveryStart,
    [property: JsonPropertyName("deliverystop")]
    string DeliveryStop,
    [property: JsonPropertyName("enable")] int Enable,
    [property: JsonPropertyName("status")] string Status
);

public sealed record JpoOdeLatLon(
    [property: JsonPropertyName("latitude")]
    string Latitude,
    [property: JsonPropertyName("longitude")]
    string Longitude
);

public sealed record JpoOdeServiceRegion(
    [property: JsonPropertyName("nwCorner")]
    JpoOdeLatLon NwCorner,
    [property: JsonPropertyName("seCorner")]
    JpoOdeLatLon SeCorner
);

public sealed record JpoOdeSdw(
    [property: JsonPropertyName("deliverystart")]
    string DeliveryStart,
    [property: JsonPropertyName("deliverystop")]
    string DeliveryStop,
    [property: JsonPropertyName("groupID")]
    string GroupId,
    [property: JsonPropertyName("recordID")]
    string RecordId,
    [property: JsonPropertyName("ttl")] string TimeToLive,
    [property: JsonPropertyName("serviceRegion")]
    JpoOdeServiceRegion ServiceRegion
);

public sealed record JpoOdeTimRequest(
    [property: JsonPropertyName("rsus")] List<JpoOdeRsu> Rsus,
    [property: JsonPropertyName("snmp")] JpoOdeSnmp Snmp,
    [property: JsonPropertyName("sdw")] JpoOdeSdw? Sdw
);

public sealed record JpoOideRoadSignIdPosition(
    [property: JsonPropertyName("latitude")]
    decimal Latitude,
    [property: JsonPropertyName("longitude")]
    decimal Longitude,
    [property: JsonPropertyName("elevation")]
    decimal Elevation
);

public sealed record JpoOideRoadSignId(
    [property: JsonPropertyName("position")]
    JpoOideRoadSignIdPosition Position,
    [property: JsonPropertyName("viewAngle")]
    string ViewAngle,
    [property: JsonPropertyName("mutcdCode")]
    string? MutcdCode,
    [property: JsonPropertyName("crc")] string? Crc
);

public sealed record JpoOdeMessageId(
    [property: JsonPropertyName("roadSignID")]
    JpoOideRoadSignId RoadSignId,
    [property: JsonPropertyName("furtherInfoID")]
    string? FurtherInfoId
);

public sealed record JpoOdeRegion(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("regulatorID")] string? RegulatorID,
    [property: JsonPropertyName("segmentID")] string? SegmentID,
    [property: JsonPropertyName("anchorPosition")] JpoOideRoadSignIdPosition? AnchorPosition,
    [property: JsonPropertyName("laneWidth")] decimal? LaneWidth,
    [property: JsonPropertyName("directionality")] int? Directionality,
    [property: JsonPropertyName("closedPath")] string? ClosedPath,
    [property: JsonPropertyName("direction")] string? Direction,
    // Valid values are "path" OR "geometry". oldregion is not supported
    [property: JsonPropertyName("description")] string? Description
);

public sealed record JpoOdeDataFrame(
    [property: JsonPropertyName("sspTimRights")]
    short SspTimRights,
    [property: JsonPropertyName("frameType")]
    string FrameType,
    [property: JsonPropertyName("msgId")] JpoOdeMessageId MsgId,
    [property: JsonPropertyName("startDateTime")]
    string StartDateTime,
    [property: JsonPropertyName("durationTime"), Range(0, 3200)]
    int DurationTime,
    [property: JsonPropertyName("priority")]
    int Priority,
    [property: JsonPropertyName("sspLocationRights")]
    int SspLocationRights,
    
    [property: JsonPropertyName("regions")]
    List<JpoOdeRegion>? Regions,

    // TODO: Do we need the regions field?
    [property: JsonPropertyName("sspMsgTypes")]
    int SspMsgType,
    [property: JsonPropertyName("sspMsgContent")]
    int SspMsgContent,
    [property: JsonPropertyName("content")]
    string Content,
    // Objects must be strings for text messages or integers for ITIS codes
    [property: JsonPropertyName("items")] List<object> Items,
    [property: JsonPropertyName("url")] string? Url
);

public sealed record JpoOdeTimMessages(
    [property: JsonPropertyName("msgCnt"), Range(0, 127)]
    int MsgCnt,
    [property: JsonPropertyName("timeStamp")]
    string TimeStamp,
    [property: JsonPropertyName("packetID")]
    string? PacketId,
    [property: JsonPropertyName("urlB")]
    string? UrlB,
    [property: JsonPropertyName("dataframes")]
    List<JpoOdeDataFrame> DataFrames
);

#region Requests

public sealed record JpoOdeTimPost(
    [property: JsonPropertyName("request")]
    JpoOdeTimRequest Request,
    [property: JsonPropertyName("tim")] JpoOdeTimMessages Tim
);

public sealed record JpoOdeTimPut(
    [property: JsonPropertyName("request")]
    JpoOdeTimRequest Request,
    [property: JsonPropertyName("tim")] JpoOdeTimMessages Tim
);

public sealed record JpoOdeTimQuery(
    [property: JsonPropertyName("rSU")] JpoOdeRsu Rsu
);

#endregion

#region Responses

public sealed record JpoOdeErrorResponse(
    [property: JsonPropertyName("error")] string Error
);

public sealed record JpoOdePostPutResponse(
    [property: JsonPropertyName("success")]
    string? Success,
    [property: JsonPropertyName("warning")]
    string? Warning
);

sealed record JpoOdeDeleteRawResponse(
    [property: JsonPropertyName("deleted_msg")]
    string DeletedMsg
);

public sealed record JpoOdeDeleteResponse(
    int DeletedMsg
);

public sealed record JpoOdeQueryRawResponse(
    string IndicesSet
);

public sealed record JpoOdeQueryResponse(
    IReadOnlySet<int> IndicesSet
);

#endregion
