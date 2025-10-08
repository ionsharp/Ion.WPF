using System;

namespace Ion.Validation;

public class Require(Exception e = null)
    : Exception("This field is required.", e);

public class RequireSelection(Exception e = null)
    : Exception("A selection is required.", e);

public class RequireValidSelection(Exception e = null)
    : Exception("A valid selection is required.", e);