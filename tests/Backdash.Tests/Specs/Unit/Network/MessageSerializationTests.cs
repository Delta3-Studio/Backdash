using Backdash.Network.Messages;
using Backdash.Tests.TestUtils;

namespace Backdash.Tests.Specs.Unit.Network;

public class MessageSerializationTests
{
    [PropertyTest]
    internal bool ConnectStatusSerialize(ConnectStatus value) =>
        AssertThat.Serialization.IsValid(ref value,
            (ref v, w) => v.Serialize(w),
            (ref v, r) => v.Deserialize(r)
        );

    [PropertyTest]
    internal bool HeaderSerialize(Header value) =>
        AssertThat.Serialization.IsValid(ref value,
            (ref v, w) => v.Serialize(w),
            (ref v, r) => v.Deserialize(r)
        );

    [PropertyTest]
    internal bool InputAckSerialize(InputAck value) =>
        AssertThat.Serialization.IsValid(ref value,
            (ref v, w) => v.Serialize(w),
            (ref v, r) => v.Deserialize(r)
        );

    [PropertyTest]
    internal bool QualityReportSerialize(QualityReport value) =>
        AssertThat.Serialization.IsValid(ref value,
            (ref v, w) => v.Serialize(w),
            (ref v, r) => v.Deserialize(r)
        );

    [PropertyTest]
    internal bool QualityReplySerialize(QualityReply value) =>
        AssertThat.Serialization.IsValid(ref value,
            (ref v, w) => v.Serialize(w),
            (ref v, r) => v.Deserialize(r)
        );

    [PropertyTest]
    internal bool SyncReplySerialize(SyncReply value) =>
        AssertThat.Serialization.IsValid(ref value,
            (ref v, w) => v.Serialize(w),
            (ref v, r) => v.Deserialize(r)
        );

    [PropertyTest]
    internal bool SyncRequestSerialize(SyncRequest value) =>
        AssertThat.Serialization.IsValid(ref value,
            (ref v, w) => v.Serialize(w),
            (ref v, r) => v.Deserialize(r)
        );

    [PropertyTest]
    internal bool InputMsgSerialize(InputMessage value) =>
        AssertThat.Serialization.IsValid(ref value,
            (ref v, w) => v.Serialize(w),
            (ref v, r) => v.Deserialize(r)
        );

    [PropertyTest]
    internal bool ConsistencyCheckRequestSerialize(ConsistencyCheckRequest value) =>
        AssertThat.Serialization.IsValid(ref value,
            (ref v, w) => v.Serialize(w),
            (ref v, r) => v.Deserialize(r)
        );

    [PropertyTest]
    internal bool ConsistencyCheckReplySerialize(ConsistencyCheckReply value) =>
        AssertThat.Serialization.IsValid(ref value,
            (ref v, w) => v.Serialize(w),
            (ref v, r) => v.Deserialize(r)
        );

    [PropertyTest]
    internal bool ConsistencyCheckFailSerialize(ConsistencyCheckFail value) =>
        AssertThat.Serialization.IsValid(ref value,
            (ref v, w) => v.Serialize(w),
            (ref v, r) => v.Deserialize(r)
        );
}
