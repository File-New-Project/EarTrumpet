﻿using System;

namespace EarTrumpet.DataModel.AppInformation;

public interface IAppInfo
{
    event Action<IAppInfo> Stopped;
    string AppId { get; }
    string DisplayName { get; }
    string ExeName { get; }
    string PackageInstallPath { get; }
    string SmallLogoPath { get; }
    bool IsDesktopApp { get; }
}
