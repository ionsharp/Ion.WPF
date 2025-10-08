namespace Ion.Colors;

public sealed class GradientPreview(GradientStepCollection steps) : object()
{
    public GradientStepCollection Steps { get; private set; } = steps;
}

/*
[Style(NameHide = true, 
    Template = nameof(ObjectControlTemplate.GradientPreview), 
    TemplateType = typeof(ObjectControlTemplate))]
public GradientPreview Preview => new(this);
*/