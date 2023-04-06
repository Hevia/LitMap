using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.KernelExtensions;
using Microsoft.SemanticKernel.Orchestration;
using Papers;

var kernel = Kernel.Builder.Build();

// Import our Semantic Skills
var keywordSkills = kernel.ImportSemanticSkillFromDirectory("Skills", "Keywords");
var paperSkills = kernel.ImportSemanticSkillFromDirectory("Skills", "Papers");
var generateSkills = kernel.ImportSemanticSkillFromDirectory("Skills", "Generate");

// Import our Native Skills
var fetchTypesSkill = kernel.ImportSkill(new FetchPapers(), "FetchPapers");

// Init our context with the research question