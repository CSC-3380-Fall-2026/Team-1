-- Stores players scores and gameplay results for each hole

CREATE TABLE PlayerScore ( 
    score_id INTEGER PRIMARY KEY,
    player_name TEXT NOT NULL,
    course_id INTEGER NOT NULL,
    hole_id INTEGER NOT NULL,
    strokes INTEGER NOT NULL,
    completion_time REAL NOT NULL
);

-- Adds a sample player score to verify the table works correctly
INSERT INTO PlayerScore (

    score_id,
    player_name,
    course_id,
    hole_id,
    strokes,
    completion_time

)

VALUES ( 

    1, 
    'Test Player',
    1,
    1,
    4,
    32.57

);

-- Verifies that the sample player score was added correctly

SELECT * FROM PlayerScore;