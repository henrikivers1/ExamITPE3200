namespace GalacticSlicer.ViewModels;

// One practice round: random questions from a single subject
public class PracticeViewModel
{
    public string Subject { get; set; } = string.Empty;
    public List<PracticeQuestionViewModel> Questions { get; set; } = new();
}

// A practice question with its answer, so the page can show right/wrong and the explanation straight away
public class PracticeQuestionViewModel
{
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int CorrectIndex { get; set; }
    public string? Explanation { get; set; }
}
