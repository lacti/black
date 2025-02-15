using System;
using MessagePack;

[Serializable]
[MessagePackObject]
public class DailyRewardData : IBlackReward
{
    [Key(4)]
    public ScInt amount;

    [Key(0)]
    public ScInt day;

    [Key(2)]
    public ScString desc;

    [Key(6)]
    public bool hidden;

    [Key(3)]
    public ScString notificationDesc;

    [Key(1)]
    public RewardType rewardType;

    [Key(5)]
    public ScString sprite;

    [Key(7)]
    public bool stashedRedeemed;

    [IgnoreMember]
    public RewardType RewardType => rewardType;

    [IgnoreMember]
    public ScInt Amount => amount;

    [IgnoreMember]
    public string AmountPostfixed => amount.ToInt().Postfixed();

    [IgnoreMember]
    public ScString NotificationDesc => notificationDesc;
}