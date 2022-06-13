import matplotlib.pyplot as plt
import matplotlib.patches as patches
import numpy as np

xL, yL, xAll, yAll = [], [], [], []
elements = []

fig, ax = plt.subplots()

with open("forGraphics/pointsLinear.txt") as file:
    for line in file:
        x, y = line.split()
        xL.append(float(x))
        yL.append(float(y))

with open("forGraphics/allPoints.txt") as file:
    for line in file:
        x, y = line.split()
        xAll.append(float(x))
        yAll.append(float(y))

with open("forGraphics/elements.txt") as file:
    for line in file:
        vert1, vert2, vert3 = map(int, line.split())
        elements.append([vert1, vert2, vert3])

for elem in elements:
    triangle = patches.Polygon([
        [xL[elem[0]], yL[elem[0]]],
        [xL[elem[1]], yL[elem[1]]],
        [xL[elem[2]], yL[elem[2]]]
    ],
        edgecolor='black', facecolor='white', linewidth=2)
    ax.add_patch(triangle)

# for i in range(len(xAll)):
#     ax.annotate(i, xy = (xAll[i] + 0.01, yAll[i] + 0.01))

plt.plot(xL, yL, " ")
plt.show()
# plt.savefig("images/4.eps")
