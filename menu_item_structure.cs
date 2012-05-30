var menu = new Menu {
	Name = "Training",
	Url = "Training",
	UserRoles = new[] { "Manager", "Officer" },

	MenuItems = new[] {
		new Menu {
			Name = "Create",
			Url = "Training/Create",
			UserRoles = new [] { "Officer" }
		},
		new Menu {
			Name = "List", 
			Url = "Training/List",
			UserRoles = new[] { "Manager", "Officer" },
		}
	}
};