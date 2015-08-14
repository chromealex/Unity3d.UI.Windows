# TSTableView by [Tacticsoft](http://www.tacticsoft.net)#

TSTableView is a plugin for Unity 4.6's new UI system that implements a Table with an API inspired by Apple's [UITableView](https://developer.apple.com/library/ios/documentation/UIKit/Reference/UITableView_Class/index.html) while obeying the standards set by Unity 4.6's GUI system.

### Introduction ###

* Unity 4.6 introducted a new UI system, but it lacks a ready for use table component.
* This implementation is built to support tables with a large number of rows, and makes use of lazy loading and object pooling to perform well.

### Features ###

* Reusable (vertical) Table component, following MVC paradigm
* No external dependencies, very small footprint
* Can handle tables with large amounts of rows
* Native iOS / Mac developers will feel at home

### Setting up ###

* If you would like to get a standalone Unity project of this component, consider cloning [TSTableViewPackage](https://bitbucket.org/tacticsoft/tstableviewpackage) instead. Don't forget to update submodules for the code itself to be included.
* This repository contains just the code assets, so you can submodule / clone it directly into a directory of choice in your unity Assets directory.
* You can also [download the repository](https://bitbucket.org/tacticsoft/tstableview/downloads) and place the files in your project.
* Open the Examples directory to see example uses of the component.

### Code Tutorial ###

* The main component introduced is the *TableView* component. The rows inside the table view are created programmatically by the data source which creates generates *TableViewCells* when asked.
* The intended usage of this component is to implement *ITableViewDataSource* with one behavior (the controller), and subclass *TableViewCell* (the view). It makes sense to create a prefab of the game object hierarchy containing the *TableViewCell* and instantiate that from the *GetCellForRowInTableView* call. Make sure to check for reusable cells before instantiating again.
* The TableView component assumes a certain hierarchy structure, see the *TableView Template* prefab for details.
* *TableView* should be placed later than "Default" in Script Execution Order

### Missing features ###

* Currently only vertical tables are supported, with one item per row
* Performance can be better, but is already good enough for thousands of rows.
* The VerticalLayoutGroup's spacing property can't change during runtime and must be smaller than the row height

### Contribution guidelines ###

* Create pull requests!

### Who do I talk to? ###

* Email [tech@tacticsoft.net](mailto:tech@tacticsoft.net) with comments, requests and suggestions
* [@noamgat](http://www.twitter.com/noamgat)