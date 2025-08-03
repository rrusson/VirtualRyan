// Configuration for personalizing the resume chat application
export interface PersonConfig {
	firstName: string;
	lastName: string;
	welcomeMsg: string;
	subtitle: string;
	avatarUrl: string;
}

// Default configuration - modify these values to customize the app
export const defaultPersonConfig: PersonConfig = {
	firstName: "Ryan",
	lastName: "Russon",
	welcomeMsg: "Hi! I'm a bot that answers questions about Ryan Russon's resume and qualifications. What would you like to know?",
	subtitle: "Representing Ryan Russon",
	avatarUrl: "https://avatars.githubusercontent.com/u/653188?v=4",
};

// Helper function to get person config.
export const getPersonConfig = (): PersonConfig => defaultPersonConfig;