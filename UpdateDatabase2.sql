-- 1. Create the SavingsGoals table
CREATE TABLE [SavingsGoals] (
    [GoalId] int NOT NULL IDENTITY,
    [Title] nvarchar(100) NOT NULL,
    [TargetAmount] decimal(18,2) NOT NULL,
    [CurrentAmount] decimal(18,2) NOT NULL,
    [TargetDate] datetime2 NOT NULL,
    [UserId] int NOT NULL,
    CONSTRAINT [PK_SavingsGoals] PRIMARY KEY ([GoalId]),
    CONSTRAINT [FK_SavingsGoals_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_SavingsGoals_UserId] ON [SavingsGoals] ([UserId]);
GO
