// Configuration for personalizing the resume chat application
export interface PersonConfig {
	firstName: string;
	lastName: string;
	welcomeMsg: string;
	subtitle: string;
	avatarUrl: string;
	copyright: string;
}

// Default configuration - modify these values to customize the app
export const defaultPersonConfig: PersonConfig = {
	firstName: "Ryan",
	lastName: "Russon",
	welcomeMsg: "Hi! I'm a bot that answers questions about Ryan Russon's resume and qualifications. What would you like to know?",
	subtitle: "Rep. Ryan Russon",
	avatarUrl: "https://avatars.githubusercontent.com/u/653188?v=4",
	copyright: "r.russon consulting"
};

// Helper function to get person config.
export const getPersonConfig = (): PersonConfig => {
	return defaultPersonConfig;
};