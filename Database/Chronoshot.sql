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

CREATE TABLE player_scores ( 
    score_id INTEGER NOT NULL AUTO_INCREMENT,
    player_name VARCHAR(100) NOT NULL,
    hits INTEGER NOT NULL,
    time_spent DECIMAL(10, 2) NOT NULL,
    score INTEGER NOT NULL,
    course_id INTEGER NOT NULL,
    hole_id INTEGER NOT NULL,
    PRIMARY KEY (score_id),
    FOREIGN KEY (course_id) REFERENCES courses(course_id),
    FOREIGN KEY (hole_id) REFERENCES holes(hole_id)
);