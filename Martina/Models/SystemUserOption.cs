﻿using Martina.DataTransferObjects;

namespace Martina.Entities;

public class SystemUserOption
{
    public const string OptionName = "SystemUser";

    public required RegisterRequest Administrator { get; set; }
}
