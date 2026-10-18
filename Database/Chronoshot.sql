-- Stores the courses available in ChronoShot

CREATE TABLE courses (

    course_id INTEGER PRIMARY KEY AUTO_INCREMENT,
    course_name TEXT NOT NULL,
    difficulty TEXT NOT NULL
);

-- Stores each hole and connects it to its course

CREATE TABLE holes (

    hole_id INTEGER PRIMARY KEY AUTO_INCREMENT, 
    course_id INTEGER NOT NULL, 
    hole_number INTEGER NOT NULL,

    FOREIGN KEY (course_id) REFERENCES courses(course_id)
); 

CREATE TABLE player_score ( 
    score_id INTEGER NOT NULL AUTO_INCREMENT,
    player_name VARCHAR(100) NOT NULL,
    course_id INTEGER NOT NULL,
    hole_id INTEGER NOT NULL,
    strokes INTEGER NOT NULL,
    completion_time DECIMAL(10, 2) NOT NULL,
    score_amount INTEGER NOT NULL,
    PRIMARY KEY (score_id),
    FOREIGN KEY (course_id) REFERENCES courses(course_id),
    FOREIGN KEY (hole_id) REFERENCES holes(hole_id)
);

-- Sample courses for the three difficulty levels 

INSERT INTO courses (course_name, difficulty) 
VALUES
    ('Beginner Course', 'Beginner'),
    ('Intermediate Course', 'Intermediate'),
    ('Extreme Course', 'Extreme');

-- Sample holes for each course
INSERT INTO holes (course_id, hole_number)
VALUES
    (1, 1),
    (1, 2),
    (1, 3),

    (2, 1),
    (2, 2),
    (2, 3),

    (3, 1),
    (3, 2),
    (3, 3);

INSERT INTO player_score (
    player_name,
    course_id,
    hole_id,
    strokes,
    completion_time,
    score_amount
)
VALUES (
    'Test Player',
    1,
    1,
    4,
    32.57,
    1000
);


-- Verify that each hole is connected to the correct course
SELECT 
    courses.course_name,
    courses.difficulty,
    holes.hole_number
FROM courses
JOIN holes ON courses.course_id = holes.course_id
ORDER BY courses.course_id, holes.hole_number;

SELECT score_id,
       player_name,
       course_id,
       hole_id,
       strokes,
       completion_time,
       score_amount
  FROM player_score;