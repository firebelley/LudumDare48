# Resource Generation

1. If coming in to the node with enough resources to build itself
   - Place buildings
1. If coming in to the node without enough resources to build itself:
   - Place villages outside any goblin camp radius, with target net resources enough to place the remaining buildings
1. If leaving node and net resources less than 6
   - Place village targeting net 6

# Goblin Camp

1. Place goblin camp in root radius such that it:
   - Does not overlap with root model
   - Does not overlap with buildings that are outside the region
1. If available resources is 10 or greater
   - Place village that generates resources within goblin camp
1. If available resources is less than 10
   - Place village outside goblin camp
1. Place root node inside goblin camp
1. Repeat, placing the next goblin camp such that it is also within the radius of the barracks
