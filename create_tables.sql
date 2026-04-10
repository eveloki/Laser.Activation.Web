-- =============================================
-- Laser Activation Web - Database Creation Script
-- Database: lead_license
-- Date: 2026-04-11
-- =============================================

SET NAMES utf8mb4;

-- --------------------------------------------------------
-- Table: Users
-- --------------------------------------------------------
CREATE TABLE IF NOT EXISTS `Users` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `Username` VARCHAR(100) NOT NULL COMMENT '用户名',
  `PasswordHash` TEXT NOT NULL COMMENT 'Argon2密码哈希',
  `Role` VARCHAR(50) NOT NULL DEFAULT 'User' COMMENT '角色: Admin / User',
  `CreatedTime` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Users_Username` (`Username`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='用户表';

-- --------------------------------------------------------
-- Alter: ActivationRecord add ActivatedByUserId
-- --------------------------------------------------------
ALTER TABLE `ActivationRecord`
  ADD COLUMN `ActivatedByUserId` INT NULL COMMENT '激活操作人用户ID' AFTER `CreatedTime`;

ALTER TABLE `ActivationRecord`
  ADD CONSTRAINT `FK_ActivationRecord_Users_ActivatedByUserId`
  FOREIGN KEY (`ActivatedByUserId`) REFERENCES `Users` (`Id`) ON DELETE SET NULL;

-- --------------------------------------------------------
-- Table: LoginLog
-- --------------------------------------------------------
CREATE TABLE IF NOT EXISTS `LoginLog` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `UserId` INT NULL COMMENT '用户ID',
  `Username` VARCHAR(100) NOT NULL COMMENT '用户名',
  `Success` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '是否成功 0/1',
  `IpAddress` VARCHAR(100) NULL COMMENT 'IP地址',
  `LoginTime` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '登录时间',
  PRIMARY KEY (`Id`),
  INDEX `IX_LoginLog_UserId` (`UserId`),
  INDEX `IX_LoginLog_LoginTime` (`LoginTime`),
  CONSTRAINT `FK_LoginLog_Users_UserId`
    FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='用户登录记录';
