 * Action to retrieve quiz/assessment results from LearnWorlds
@RegisterClass(BaseAction, 'GetQuizResultsAction')
export class GetQuizResultsAction extends LearnWorldsBaseAction {
     * Get quiz results for a user, course, or specific quiz
            const quizId = this.getParamValue(Params, 'QuizID');
            const includeQuestions = this.getParamValue(Params, 'IncludeQuestions') !== false;
            const includeAnswers = this.getParamValue(Params, 'IncludeAnswers') !== false;
            const passingOnly = this.getParamValue(Params, 'PassingOnly') === true;
            const sortBy = this.getParamValue(Params, 'SortBy') || 'completed_at';
            // Require at least one identifier
            if (!userId && !courseId && !quizId) {
                    Message: 'At least one of UserID, CourseID, or QuizID is required',
            if (passingOnly) {
                queryParams.passed = true;
                queryParams.completed_after = new Date(dateFrom).toISOString();
                queryParams.completed_before = new Date(dateTo).toISOString();
            let endpoint = '/quiz-results';
            if (quizId) {
                endpoint = `/quizzes/${quizId}/results`;
                if (userId) {
                    queryParams.user_id = userId;
            } else if (userId && courseId) {
                endpoint = `/users/${userId}/courses/${courseId}/quiz-results`;
                endpoint = `/users/${userId}/quiz-results`;
                endpoint = `/courses/${courseId}/quiz-results`;
            // Get quiz results
            const resultsResponse = await this.makeLearnWorldsRequest(
            if (!resultsResponse.success) {
                    Message: resultsResponse.message || 'Failed to retrieve quiz results',
            const results = resultsResponse.data?.data || resultsResponse.data || [];
            const formattedResults: any[] = [];
            // Process each quiz result
                const formattedResult: any = {
                    id: result.id || result.result_id,
                    userId: result.user_id || userId,
                    courseId: result.course_id || courseId,
                    quizId: result.quiz_id || quizId,
                    attemptNumber: result.attempt_number || result.attempt || 1,
                    score: result.score || 0,
                    maxScore: result.max_score || result.total_points || 100,
                    percentage: result.percentage || ((result.score / (result.max_score || 100)) * 100),
                    passed: result.passed || result.is_passing || false,
                    passingScore: result.passing_score || result.passing_percentage || 70,
                    startedAt: result.started_at,
                    completedAt: result.completed_at || result.submitted_at,
                    duration: result.duration || result.time_spent,
                    durationText: this.formatDuration(result.duration || result.time_spent || 0)
                if (result.user && !formattedResult.user) {
                    formattedResult.user = {
                        id: result.user.id,
                        email: result.user.email,
                        name: result.user.name || `${result.user.first_name || ''} ${result.user.last_name || ''}`.trim()
                // Add quiz info if available
                if (result.quiz) {
                    formattedResult.quiz = {
                        id: result.quiz.id,
                        title: result.quiz.title || result.quiz.name,
                        type: result.quiz.type || 'quiz',
                        questionCount: result.quiz.question_count || result.quiz.total_questions
                } else if (!quizId && result.quiz_id) {
                    // Try to get quiz info
                    const quizResponse = await this.makeLearnWorldsRequest(
                        `/quizzes/${result.quiz_id}`,
                    if (quizResponse.success && quizResponse.data) {
                        const quiz = quizResponse.data;
                            id: quiz.id,
                            title: quiz.title || quiz.name,
                            type: quiz.type || 'quiz',
                            questionCount: quiz.question_count || quiz.total_questions
                // Add questions and answers if requested
                if (includeQuestions || includeAnswers) {
                    const detailsResponse = await this.makeLearnWorldsRequest(
                        `/quiz-results/${formattedResult.id}/details`,
                    if (detailsResponse.success && detailsResponse.data) {
                        const details = detailsResponse.data;
                        if (includeQuestions) {
                            formattedResult.questions = this.formatQuestions(details.questions || details.answers || []);
                        if (includeAnswers) {
                            formattedResult.answers = this.formatAnswers(details.answers || details.questions || []);
                            // Calculate additional metrics
                            const answers = formattedResult.answers || [];
                            formattedResult.metrics = {
                                correctAnswers: answers.filter((a: any) => a.isCorrect).length,
                                incorrectAnswers: answers.filter((a: any) => !a.isCorrect).length,
                                totalQuestions: answers.length,
                                accuracyRate: answers.length > 0 
                                    ? (answers.filter((a: any) => a.isCorrect).length / answers.length * 100).toFixed(1)
                formattedResults.push(formattedResult);
            const totalResults = formattedResults.length;
            const passedResults = formattedResults.filter(r => r.passed).length;
            const averageScore = totalResults > 0 
                ? formattedResults.reduce((sum, r) => sum + r.percentage, 0) / totalResults 
            const averageDuration = totalResults > 0
                ? formattedResults.reduce((sum, r) => sum + (r.duration || 0), 0) / totalResults
            // Group results by quiz if multiple quizzes
            const resultsByQuiz: any = {};
            if (!quizId) {
                formattedResults.forEach(result => {
                    const quizTitle = result.quiz?.title || 'Unknown Quiz';
                    if (!resultsByQuiz[quizTitle]) {
                        resultsByQuiz[quizTitle] = {
                                attempts: 0,
                                averageScore: 0,
                                passRate: 0
                    resultsByQuiz[quizTitle].results.push(result);
                // Calculate stats for each quiz
                Object.keys(resultsByQuiz).forEach(quizTitle => {
                    const quizResults = resultsByQuiz[quizTitle].results;
                    resultsByQuiz[quizTitle].stats = {
                        attempts: quizResults.length,
                        averageScore: (quizResults.reduce((sum: number, r: any) => sum + r.percentage, 0) / quizResults.length).toFixed(1),
                        passRate: ((quizResults.filter((r: any) => r.passed).length / quizResults.length) * 100).toFixed(1)
                totalResults: totalResults,
                passedResults: passedResults,
                failedResults: totalResults - passedResults,
                passRate: totalResults > 0 ? ((passedResults / totalResults) * 100).toFixed(1) : 0,
                averageScore: averageScore.toFixed(1),
                averageDuration: averageDuration,
                averageDurationText: this.formatDuration(Math.round(averageDuration)),
                filterType: userId ? (courseId ? 'user-course' : 'user') : (courseId ? 'course' : 'all'),
                quizBreakdown: !quizId ? resultsByQuiz : null
            const quizResultsParam = outputParams.find(p => p.Name === 'QuizResults');
            if (quizResultsParam) {
                quizResultsParam.Value = formattedResults;
                totalCountParam.Value = totalResults;
                Message: `Retrieved ${totalResults} quiz result(s)`,
                Message: `Error retrieving quiz results: ${errorMessage}`,
     * Format quiz questions
    private formatQuestions(questions: any[]): any[] {
        return questions.map((q, index) => ({
            questionNumber: q.question_number || index + 1,
            questionId: q.question_id || q.id,
            questionText: q.question_text || q.question || q.text,
            questionType: q.question_type || q.type || 'multiple-choice',
            points: q.points || q.score || 1,
            difficulty: q.difficulty || 'medium'
     * Format user answers
    private formatAnswers(answers: any[]): any[] {
        return answers.map((a, index) => ({
            questionNumber: a.question_number || index + 1,
            questionId: a.question_id,
            userAnswer: a.user_answer || a.answer || a.selected_answer,
            correctAnswer: a.correct_answer,
            isCorrect: a.is_correct || a.correct || false,
            pointsEarned: a.points_earned || (a.is_correct ? (a.points || 1) : 0),
            pointsPossible: a.points_possible || a.points || 1,
            feedback: a.feedback,
            timeSpent: a.time_spent,
            answeredAt: a.answered_at
                Name: 'QuizID',
                Name: 'IncludeQuestions',
                Name: 'IncludeAnswers',
                Name: 'PassingOnly',
                Value: 'completed_at'
                Name: 'QuizResults',
        return 'Retrieves quiz and assessment results from LearnWorlds with detailed question/answer information';
