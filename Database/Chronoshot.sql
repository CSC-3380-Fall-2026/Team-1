-- Stores the courses available in ChronoShot

CREATE TABLE Course (

    course_id INTEGER PRIMARY KEY,
    course_name TEXT NOT NULL,
    difficulty TEXT NOT NULL
);

-- Stores each hole and connects it to its course

CREATE TABLE Hole (

    hole_id INTEGER PRIMARY KEY, 
    course_id INTEGER NOT NULL, 
    hole_number INTEGER NOT NULL,

    FOREIGN KEY (course_id) REFERENCES Course(course_id)
); 

-- Sample courses for the three difficulty levels 

INSERT INTO Course (course_id, course_name, difficulty) 
VALUES
    (1, 'Beginner Course', 'Beginner'),
    (2, 'Intermediate Course', 'Intermediate'),
    (3, 'Extreme Course', 'Extreme');

-- Sample holes for each course
INSERT INTO Hole (hole_id, course_id, hole_number)
VALUES
    (1, 1, 1),
    (2, 1, 2),
    (3, 1, 3),

    (4, 2, 1),
    (5, 2, 2),
    (6, 2, 3),

    (7, 3, 1),
    (8, 3, 2),
    (9, 3, 3);

-- Verify that each hole is connected to the correct course
SELECT 
    Course.course_name,
    Course.difficulty,
    Hole.hole_number
FROM Course
JOIN Hole ON Course.course_id = Hole.course_id
ORDER BY Course.course_id, Hole.hole_number;