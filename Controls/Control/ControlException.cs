using System;

namespace Ion.Controls;

public class ControlMissingParent<Child, Parent>(Exception e = null)
    : Exception($"{nameof(Child)} '{typeof(Child).FullName}' must have logical or visual parent of type '{typeof(Parent).FullName}'.", e);

public class ControlMissingParentLogical<Child, Parent>(Exception e = null)
    : Exception($"{nameof(Child)} '{typeof(Child).FullName}' must have logical parent of type '{typeof(Parent).FullName}'.", e);

public class ControlMissingParentVisual<Child, Parent>(Exception e = null)
    : Exception($"{nameof(Child)} '{typeof(Child).FullName}' must have visual parent of type '{typeof(Parent).FullName}'.", e);