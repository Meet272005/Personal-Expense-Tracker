-- 1. Create the Wallets table
CREATE TABLE [Wallets] (
    [WalletId] int NOT NULL IDENTITY,
    [Name] nvarchar(50) NOT NULL,
    [InitialBalance] decimal(18,2) NOT NULL,
    [UserId] int NOT NULL,
    CONSTRAINT [PK_Wallets] PRIMARY KEY ([WalletId]),
    CONSTRAINT [FK_Wallets_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_Wallets_UserId] ON [Wallets] ([UserId]);
GO

-- 2. Add WalletId to Transactions table
ALTER TABLE [Transactions] ADD [WalletId] int NULL;
GO

-- 3. Add Foreign Key for WalletId in Transactions
ALTER TABLE [Transactions] ADD CONSTRAINT [FK_Transactions_Wallets_WalletId] FOREIGN KEY ([WalletId]) REFERENCES [Wallets] ([WalletId]) ON DELETE SET NULL;
GO

CREATE INDEX [IX_Transactions_WalletId] ON [Transactions] ([WalletId]);
GO
