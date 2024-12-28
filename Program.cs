const float ViewBoxWidth = 3840;

/*
 * We are making a timeline which snakes left to right and then right
 * to left from 1000AD to 2000AD.
 *
 * Each century is a row, including the semicircle linking it back/to
 * the previous/next century.
 *
 * See https://docs.google.com/document/d/1i-Z9JTrO3uMBjwvASQSfgQwiONFfjexbJj-ClOJnfbo/
 */

const float Margin = 32;

const float TimelineHeight = 250;  // Main timeline height (and width when turning at the ends)
const float TimelineGap = 70;   // Gap between each century

const float TimelineWidth = ViewBoxWidth - Margin * 2; // Full width of the timeline
const float TimelineExteriorRadius = TimelineHeight + TimelineGap / 2;
const float TimelineCentreRadius = TimelineExteriorRadius - TimelineHeight / 2;
const float TimelineInteriorRadius = TimelineGap / 2;
const float TimelineLinearLength = TimelineWidth - TimelineExteriorRadius * 2;
const float TimelineCentreArcLength = MathF.PI * TimelineCentreRadius;
const float TimelineTotalLength = TimelineCentreArcLength + TimelineLinearLength;
const float TimelineDistancePerYear = (TimelineTotalLength) / 100;
const float TimelineLinearYears = TimelineLinearLength / TimelineDistancePerYear;
const float YearLabelGap = 10;
const float BorderWidth = 5f;

Console.WriteLine($"TimelineLinearLength: {TimelineLinearLength}");
Console.WriteLine($"TimelineLinearYears: {TimelineLinearYears}");
Console.WriteLine($"TimelineDistancePerYear: {TimelineDistancePerYear}");
Console.WriteLine($"TimelineCentreRadius: {TimelineCentreRadius}");
Console.WriteLine($"TimelineCentreArcLength: {TimelineCentreArcLength}");
Console.WriteLine($"TimelineTotalLength: {TimelineTotalLength}");

const float ViewBoxHeight = Margin * 2 + TimelineHeight * 11 + TimelineGap * 10;

string timelineSvgPrefix = $"""
                            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {ViewBoxWidth} {ViewBoxHeight}">
                            """;
const string timelineSvgSuffix = "</svg>";

var timelineSvgFilename = "timeline.svg";

var elements = new List<string>();
RenderTimeline(1066, 2024, TimelineHeight, 0);

RenderTimeline(1914, 1918, TimelineHeight * 0.75f, 0);

RenderTimeline(1939, 1945, TimelineHeight * 0.75f, 0);

//RenderTimeline(1080, 1120, TimelineHeight, 0);
//RenderTimeline(1180, 1220, TimelineHeight, 0);

for (int i = 1000; i < 2025; i++)
{
    RenderYearPoint(i, 0);
    RenderYearPoint(i, -1);
    RenderYearPoint(i, 1);
}

foreach (var year in new[] { 1066, 1215, 1348, 1492, 1776, 1914, 1945, 2024 })
{
    RenderYearLabel(year);
}

for (int year = 1000; year < 2000; year += 2)
{
    RenderYearLabel(year, 0);
}

var svg = $"{timelineSvgPrefix}\n{string.Join("\n", elements)}\n{timelineSvgSuffix}";

Console.WriteLine(svg);

File.WriteAllText(timelineSvgFilename, svg);
return;

(float,float) CalcYearPos(float year, float offset)
{
    year -= 1000;
    int centuryDiv = (int) Math.Floor(year / 100);
    var centuryMod = year - centuryDiv * 100;

    float x = Margin + TimelineExteriorRadius;
    float y = Margin + TimelineHeight / 2 + (TimelineHeight + TimelineGap) * centuryDiv;
    
    var oddCentury = centuryDiv % 2 == 1;
    var direction = 1;
    if (oddCentury)
    {
        x += TimelineLinearLength;
        direction = -1;
    }

    if (centuryMod < TimelineLinearYears)
    {
        return (centuryMod * TimelineDistancePerYear * direction + x, y);
    }
    
    x += TimelineLinearLength * direction;
    y += TimelineCentreRadius;

    float angle = 180 * (centuryMod - TimelineLinearYears) * TimelineDistancePerYear / TimelineCentreArcLength;
    float radians = MathF.PI * angle / 180;
    
    return (x + MathF.Sin(radians) * TimelineCentreRadius * direction, y - MathF.Cos(radians) * TimelineCentreRadius);
}

string StartCap(float fromYear, float height, float offset)
{
    var (x, y) = CalcYearPos(fromYear, offset);
    return $"d=\"M {x},{y}";
}

string Middle(float fromYear, float toYear, float height, float offset)
{
    string middle = "";
    float year = MathF.Ceiling(fromYear);
    while (year < toYear)
    {
        if (middle == "")
            middle += "C ";
        else
            middle += ",";

        var (x, y) = CalcYearPos(year, offset);
        middle += $" {x},{y}";
        year += 0.25f;
    }
    return middle;
}

string EndCap(float fromYear, float height, float offset)
{
    var (x, y) = CalcYearPos(fromYear, offset);
    return $", {x},{y} \"";
}

void RenderCapsule(float fromYear, float toYear, float height, float offset, string color)
{
    var timeline = $"<path style=\"fill:none;stroke:{color};stroke-width:{height};stroke-linecap:round\"\n";
    timeline += StartCap(fromYear, height, offset);
    timeline += Middle(fromYear, toYear, height, offset);
    timeline += EndCap(toYear, height, offset);
    timeline += "/>";

    elements.Add(timeline);
}

void RenderTimeline(float fromYear, float toYear, float height, float offset)
{
    RenderCapsule(fromYear, toYear, height, offset, "black");
    RenderCapsule(fromYear, toYear, height - BorderWidth, offset, "#f05010");
}

void RenderYearLabel(float year, float offset = 0)
{
    var (x, y) = CalcYearPos(year, offset);
    y -= TimelineHeight / 2 + YearLabelGap;
    elements.Add($"<text x=\"{x}\" y=\"{y}\" font-family=\"Arial\" font-size=\"14\" text-anchor=\"middle\" fill=\"black\">{year}</text>");
}

void RenderYearPoint(float year, float offset)
{
    var (x, y) = CalcYearPos(year, offset);
    elements.Add($"<circle cx=\"{x}\" cy=\"{y}\" r=\"2\" fill=\"black\" />");
}