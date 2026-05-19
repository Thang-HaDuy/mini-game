Chỉ cần vào Inspector của WorldGenerator, thêm phần tử vào mảng Biomes và điền:

Field	Ý nghĩa
biomeName	Tên biome (debug)
noiseMin/Max	Khoảng noise [0–1] biome này chiếm
topBlock	Block trên bề mặt
subSurfaceBlock	3 lớp dưới bề mặt
fillBlock	Phần còn lại
baseBlock	Lớp y=0
Ví dụ 2 biome cũ giờ được cấu hình bằng 2 phần tử trong array thay vì hardcode trong code.

6 Biome mẫu cho Inspector
#	biomeName	noiseMin	noiseMax	topBlock	subSurfaceBlock	fillBlock	baseBlock
1	Plains	0.00	0.17	4	2	3	1
2	Forest	0.17	0.33	4	2	2	1
3	Desert	0.33	0.50	3	3	2	1
4	Mountain	0.50	0.67	1	1	3	1
5	Tundra	0.67	0.83	3	2	3	1
6	Volcanic	0.83	1.00	1	3	1	1