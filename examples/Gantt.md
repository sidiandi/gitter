# Gantt Diagram

This is only a proposal and subject to change.

You are very welcome [[http://forum.plantuml.net|to create a new discussion]] on this future syntax. Your feedbacks, ideas and suggestions help us to find the right solution.

The Gantt is described in //natural// language, using very simple sentences (subject-verb-complement). 

## Declaring tasks

Tasks defined using square bracket. Their durations are defined using the ''last'' verb:


```plantuml
@startgantt
[Prototype design] lasts 15 days
[Test prototype] lasts 10 days
@endgantt
```

## Adding constraints
It is possible to add constraints between task. 

```plantuml
@startgantt
[Prototype design] lasts 15 days
[Test prototype] lasts 10 days
[Test prototype] starts at [Prototype design]'s end
@endgantt
```

```plantuml
@startgantt
[Prototype design] lasts 10 days
[Code prototype] lasts 10 days
[Write tests] lasts 5 days
[Code prototype] starts at [Prototype design]'s end
[Write tests] starts at [Code prototype]'s start
@endgantt
```

## Short names
It is possible to define short name for tasks with the ''as'' keyword. 

```plantuml
@startgantt
[Prototype design] as [D] lasts 15 days
[Test prototype] as [T] lasts 10 days
[T] starts at [D]'s end
@endgantt
```

## Customize colors
It also possible to customize [[https://www.w3schools.com/colors/colors_names.asp|colors]]. 

```plantuml
@startgantt
[Prototype design] lasts 13 days
[Test prototype] lasts 4 days
[Test prototype] starts at [Prototype design]'s end
[Prototype design] is colored in Fuchsia/FireBrick 
[Test prototype] is colored in GreenYellow/Green 
@endgantt
```

## Milestone
You can define Milestones using the ''happens'' verb. 

```plantuml
@startgantt
[Test prototype] lasts 10 days
[Prototype completed] happens at [Test prototype]'s end
[Setup assembly line] lasts 12 days
[Setup assembly line] starts at [Test prototype]'s end
@endgantt
```

## Calendar
You can specify a starting date for the whole project. By default, the first task starts at this date. 

```plantuml
@startgantt
Project starts the 20th of september 2017
[Prototype design] as [TASK1] lasts 13 days
[TASK1] is colored in Lavender/LightBlue
@endgantt
```

## Close day
It is possible to close some day.

```plantuml
@startgantt
project starts the 2018/04/09
saturday are closed
sunday are closed
2018/05/01 is closed
2018/04/17 to 2018/04/19 is closed
[Prototype design] lasts 14 days
[Test prototype] lasts 4 days
[Test prototype] starts at [Prototype design]'s end
[Prototype design] is colored in Fuchsia/FireBrick 
[Test prototype] is colored in GreenYellow/Green 
@endgantt
```

## Simplified task succession
It's possible to use the ''then'' keyword to denote consecutive tasks.

```plantuml
@startgantt
[Prototype design] lasts 14 days
then [Test prototype] lasts 4 days
then [Deploy prototype] lasts 6 days
@endgantt
```

You can also use arrow ''%%->%%''

```plantuml
@startgantt
[Prototype design] lasts 14 days
[Build prototype] lasts 4 days
[Prepare test] lasts 6 days
[Prototype design] -> [Build prototype]
[Prototype design] -> [Prepare test]
@endgantt
```

## Grouping

**Important warning** : this feature may be removed in future version, so please do not use in long term diagrams.

You can add a prefix in the form of ''prefix/'' to a task's name in order to group them together.

```plantuml
@startgantt
[Group1/Group1 Task1] lasts 5 days and is colored in Fuchsia/FireBrick
[Group2/Group2 Task1] lasts 7 days and is colored in GreenYellow/Green
[Group2/Group2 Task2] lasts 5 days and is colored in GreenYellow/Green
[Group1/Group1 Task2] lasts 7 days and is colored in Fuchsia/FireBrick
@endgantt
```

## Separator

You can use ''%%--%%'' to separate sets of tasks.

```plantuml
@startgantt
[Task1] lasts 10 days
then [Task2] lasts 4 days
-- Phase Two --
then [Task3] lasts 5 days
then [Task4] lasts 6 days
@endgantt
```

## Working with resources
You can affect tasks on resources using the ''on'' keyword and brackets for resource name.

```plantuml
@startgantt
[Task1] on {Alice} lasts 10 days
[Task2] on {Bob} lasts 2 days at 50% 
then [Task3] on {Alice} lasts 1 days at 25%
@endgantt
```

## Complex example
It also possible to use the ''and'' conjunction.

You can also add delays in constraints. 

```plantuml
@startgantt
[Prototype design] lasts 13 days and is colored in Lavender/LightBlue
[Test prototype] lasts 9 days and is colored in Coral/Green and starts 3 days after [Prototype design]'s end
[Write tests] lasts 5 days and ends at [Prototype design]'s end
[Hire tests writers] lasts 6 days and ends at [Write tests]'s start
[Init and write tests report] is colored in Coral/Green
[Init and write tests report] starts 1 day before [Test prototype]'s start and ends at [Test prototype]'s end
@endgantt
```
