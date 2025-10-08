using System;

namespace Ion.Core;

public class PasswordNotValid(Exception e = null) 
    : Exception("Password isn't valid.", e);