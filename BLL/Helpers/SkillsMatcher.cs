namespace BLL.Helpers;

internal static class SkillsMatcher
{
    
    internal static HashSet<string> Parse(string skills)
    {
        ArgumentNullException.ThrowIfNull(skills);

        return
        [
            .. skills
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => s.ToUpperInvariant()),
        ];
    }

    internal static bool HasOverlap(string candidateSkills, string requiredSkills)
    {
        ArgumentNullException.ThrowIfNull(candidateSkills);
        ArgumentNullException.ThrowIfNull(requiredSkills);

        HashSet<string> candidate = Parse(candidateSkills);
        return Parse(requiredSkills).Any(candidate.Contains);
    }
}
