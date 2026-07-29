CREATE TABLE dbo.RevenueDailySnapshots
(
    SnapshotDateUtc DATE NOT NULL,
    Currency CHAR(3) NOT NULL,
    Mrr DECIMAL(19, 2) NOT NULL,
    Arr DECIMAL(19, 2) NOT NULL,
    AverageRevenuePerActiveSubscription DECIMAL(19, 2) NOT NULL,
    ActiveSubscriptions INT NOT NULL,
    CapturedUtc DATETIME2 NOT NULL,
    CONSTRAINT PK_RevenueDailySnapshots PRIMARY KEY (SnapshotDateUtc),
    CONSTRAINT CK_RevenueDailySnapshots_Currency CHECK (Currency = 'USD'),
    CONSTRAINT CK_RevenueDailySnapshots_Mrr CHECK (Mrr >= 0),
    CONSTRAINT CK_RevenueDailySnapshots_Arr CHECK (Arr >= 0),
    CONSTRAINT CK_RevenueDailySnapshots_AverageRevenue CHECK (AverageRevenuePerActiveSubscription >= 0),
    CONSTRAINT CK_RevenueDailySnapshots_ActiveSubscriptions CHECK (ActiveSubscriptions >= 0)
);
