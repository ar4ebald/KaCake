import random
import sys
from habitat_drawer import *

class habitat(object):
    CELL_EMPTY = 0
    CELL_PREDATOR = 1
    CELL_VICTIM = 2
    CELL_OBSTACLE = 3
    CELL_MODULO = 4

    def new_predator(self):
        return habitat.CELL_PREDATOR + habitat.CELL_MODULO * (random.randint(0, self.predators_spawn_time - 1) + self.predators_spawn_time * random.randint(0, self.max_hunger - 1))

    def new_victim(self):
        return habitat.CELL_VICTIM + habitat.CELL_MODULO * random.randint(0, self.victims_spawn_time - 1)

    def __init__(self, grid_size_x, grid_size_y, predators_count, victims_count, obstacles_count, max_hunger, predators_spawn_time, victims_spawn_time, steps_per_log):
        self.grid_size_x = grid_size_x
        self.grid_size_y = grid_size_y

        self.grid = [[habitat.CELL_EMPTY] * self.grid_size_x for y in range(self.grid_size_y)]
        self.cell_is_processed = [[False] * self.grid_size_x for y in range(self.grid_size_y)]

        self.predators_count = predators_count
        self.victims_count = victims_count
        self.obstacles_count = obstacles_count
        self.max_hunger = max_hunger
        self.predators_spawn_time = predators_spawn_time
        self.victims_spawn_time = victims_spawn_time

        self.timer = 0

        for i in range(self.grid_size_y):
            self.grid[i][0] = habitat.CELL_OBSTACLE
            self.grid[i][self.grid_size_x - 1] = habitat.CELL_OBSTACLE

        for j in range(self.grid_size_x):
            self.grid[0][j] = habitat.CELL_OBSTACLE
            self.grid[self.grid_size_y - 1][j] = habitat.CELL_OBSTACLE

        for i in range(self.predators_count):
            self.place_entity(self.new_predator())
        for i in range(self.victims_count):
            self.place_entity(self.new_victim())
        for i in range(self.obstacles_count):
            self.place_entity(habitat.CELL_OBSTACLE)

        self.file = open("data.csv", "w+")
        self.file.write("Generation;Predators;Victims\n")

        self.steps_per_log = steps_per_log

    def place_entity(self, entity):
        while True:
            x, y = random.randrange(0, self.grid_size_x), random.randrange(0, self.grid_size_y)
            if self.grid[x][y] == habitat.CELL_EMPTY:
                break
        self.grid[x][y] = entity

    def unpack_cell(self, i, j):
        cell = self.grid[i][j]
        type = cell % habitat.CELL_MODULO
        cell //= habitat.CELL_MODULO
        if type == habitat.CELL_PREDATOR:
            return [type, cell % self.predators_spawn_time, cell // self.predators_spawn_time]
        elif type == habitat.CELL_VICTIM:
            return [type, cell, None]
        else:
            return [type, None, None]

    def get_neighboring_victims(self, x, y):
        result = []
        if self.unpack_cell(y, x + 1)[0] == habitat.CELL_VICTIM:
            result.append((x + 1, y))
        if self.unpack_cell(y, x - 1)[0] == habitat.CELL_VICTIM:
            result.append((x - 1, y))
        if self.unpack_cell(y + 1, x)[0] == habitat.CELL_VICTIM:
            result.append((x, y + 1))
        if self.unpack_cell(y - 1, x)[0] == habitat.CELL_VICTIM:
            result.append((x, y - 1))
        return result

    def get_empty_neighbors(self, x, y):
        result = []
        if self.grid[y][x + 1] == habitat.CELL_EMPTY:
            result.append((x + 1, y))
        if self.grid[y][x - 1] == habitat.CELL_EMPTY:
            result.append((x - 1, y))
        if self.grid[y + 1][x] == habitat.CELL_EMPTY:
            result.append((x, y + 1))
        if self.grid[y - 1][x] == habitat.CELL_EMPTY:
            result.append((x, y - 1))
        return result

    def move(self, pos_from, pos_to):
        self.grid[pos_to[1]][pos_to[0]] = self.grid[pos_from[1]][pos_from[0]]
        self.grid[pos_from[1]][pos_from[0]] = habitat.CELL_EMPTY


    def perform_predator_step(self, x, y, breeding_timer, hunger):

        breeding_timer -= random.randint(0, 2)
        hunger -= random.randint(0, 2)

        if breeding_timer < 0:
            options = self.get_empty_neighbors(x, y) + self.get_neighboring_victims(x, y)
            if options:
                n_x, n_y = random.choice(options)
                self.grid[n_y][n_x] = self.new_predator()
                self.cell_is_processed[n_y][n_x] = True
                breeding_timer = self.predators_spawn_time - 1
            else:
                breeding_timer = 1

        options = self.get_neighboring_victims(x, y)
        if options:
            n_x, n_y = random.choice(options)
            hunger = self.max_hunger - 1
            breeding_timer = max(0, breeding_timer * 9 // 10)
            self.grid[n_y][n_x] = habitat.CELL_PREDATOR + habitat.CELL_MODULO * (breeding_timer + self.predators_spawn_time * hunger)
            self.cell_is_processed[n_y][n_x] = True
            self.grid[y][x] = habitat.CELL_EMPTY
        elif hunger >= 0:
            options = self.get_empty_neighbors(x, y)
            if options:
                n_x, n_y = random.choice(options)
                self.grid[n_y][n_x] = habitat.CELL_PREDATOR + habitat.CELL_MODULO * (breeding_timer + self.predators_spawn_time * hunger)
                self.cell_is_processed[n_y][n_x] = True
                self.grid[y][x] = habitat.CELL_EMPTY
            else:
                self.grid[y][x] = habitat.CELL_PREDATOR + habitat.CELL_MODULO * (breeding_timer + self.predators_spawn_time * hunger)
        else:
            self.grid[y][x] = habitat.CELL_EMPTY

    def perform_victim_step(self, x, y, breeding_timer):

        breeding_timer -= random.randint(0, 2)

        if breeding_timer < 0:
            options = self.get_empty_neighbors(x, y)
            if options:
                n_x, n_y = random.choice(options)
                self.grid[n_y][n_x] = self.new_victim()
                self.cell_is_processed[n_y][n_x] = True
                breeding_timer = self.victims_spawn_time - 1
            else:
                breeding_timer = 1

        options = self.get_empty_neighbors(x, y)
        if options:
            self.grid[y][x] = habitat.CELL_EMPTY
            x, y = random.choice(options)
            self.cell_is_processed[y][x] = True

        self.grid[y][x] = habitat.CELL_VICTIM + habitat.CELL_MODULO * breeding_timer


    def update(self):
        self.timer += 1

        for i in range(self.grid_size_y):
            for j in range(self.grid_size_x):
                self.cell_is_processed[i][j] = False

        predators_count = 0
        victims_count = 0

        for i in range(self.grid_size_y):
            for j in range(self.grid_size_x):
                if not self.cell_is_processed[i][j]:
                    type, breeding_timer, hunger = self.unpack_cell(i, j)
                    if type == habitat.CELL_PREDATOR:
                        predators_count += 1
                        self.perform_predator_step(j, i, breeding_timer, hunger)
                    elif type == habitat.CELL_VICTIM:
                        victims_count += 1
                        self.perform_victim_step(j, i, breeding_timer)

        if predators_count == 0:
            self.place_entity(self.new_predator())

        if self.timer % self.steps_per_log == 0:
            self.file.write("{};{};{}\n".format(self.timer, predators_count, victims_count))
            self.file.flush()

            progress_size = 50
            progress = progress_size * predators_count // (predators_count + victims_count)
            print('#' * progress + '.' * (progress_size - progress), "Gen {}\tPredators {}\tvictims{}".format(self.timer, predators_count, victims_count))

h = habitat(grid_size_x = 100,
            grid_size_y = 100,
            predators_count = 100,
            victims_count = 200,
            obstacles_count = 1000,
            max_hunger = 60,
            predators_spawn_time = 90,
            victims_spawn_time = 40,
            steps_per_log = 10)

if len(sys.argv) == 2 and sys.argv[1] == "nogui":
    while True:
        h.update()
else:
    drawer = habitat_drawer(h, 10, 1)
    drawer.run_loop(h.update)