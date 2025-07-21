// Provides utilities for handling pronunciation logic in the application.
export class Pronunciation {

    public makeTlasPhonetic(input: string): string {
        const phoneticMap: { [key: string]: string } = {
            ".NET": "dot net",
            "API": "A P I",
            "ASP.NET": "A S P dot net",
            "AWS": "A W S",
            "C#": "C sharp",
            "C++": "C plus plus",
			"CI/CD": "C I C D",
            "CSS": "C S S",
            "DB": "database",
            "DI": "dependency injection",
            "DTS": "D T S",
            "ETL": "E T L",
            "GoF": "gang of four",
            "HTML": "H T M L",
            "IoC": "inversion of control",
            "JavaScript": "java script",
            "jQuery": "jay query",
            "JSON": "jay son",
            "LINQ": "link",
            "LLM": "L L M",
            "MCP": "M C P",
            "MOQ": "M O Q",
            "MS SQL": "M S sequel",
            "MVC": "M V C",
            "MVVM": "M V V M",
            "NoSQL": "no sequel",
            "Ollama": "oh-lah-mah",
            "OOP": "O O P",
            "OpenAPI": "open A P I",
            "OWASP": "O wasp",
            "PostGres": "post gress",
            "PostGresSQL": "post gress",
            "S3": "S three",
            "SNS": "S N S",
            "SQL": "sequel",
            "SQS": "S Q S",
            "SSIS": "S S I S",
            "SSRS": "S S R S",
            "TDD": "T D D",
            "T-SQL": "T sequel",
            "TypeScript": "type script",
            "VS": "V S",
            "WCF": "W C F",
            "XAML": "zammel",
            "XML": "X M L"
        };

        // Sort keys by length descending to avoid partial replacements
        const sortedKeys = Object.keys(phoneticMap).sort((a, b) => b.length - a.length);

        let result = input;
        for (const key of sortedKeys) {
            // Use word boundaries for acronyms and phrases, case-insensitive
            const pattern = new RegExp(`\\b${key.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}\\b`, "gi");
            result = result.replace(pattern, phoneticMap[key]);
        }

        return result;
    }
}