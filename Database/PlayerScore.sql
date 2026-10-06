-- Stores players scores and gameplay results for each hole

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

-- Adds a sample player score to verify the table works correctly
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

-- Verifies that the sample player score was added correctly

SELECT 
    
    score_id, 
    player_name, 
    course_id, 
    hole_id, 
    strokes, 
    completion_time
    score_amount
FROM player_score;