The satabase can be easily extracted by running the unit tests for the database services.
upon successful runnning of a test the SQLite db will be generated in your documents folder as a file called AgentAssist.db3
This DB will have the same structure as the apps packaged DB

SQLite will perform migrations on the models passed in meaning that if the POCO changes, we only need to update the feed and rebuild the app to get the new fields added to the DB.
SQLite is however limited in that it will only performs and alter add column, so properties removed from the model wont be removed from the table, columns wont be renamed they will stay as is and a new column will be created etc etc.....

If a POCO model has a variable renamed, to update the db on the app, you will need to write a migration script to populate the new column / drop the old column and run it on startup int eh createdatabase call

i.e. _db.Execute("UPDATE User SET Column2=Column1");