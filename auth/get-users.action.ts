 * Interface for a LearnWorlds user
export interface LearnWorldsUser {
    totalCourses?: number;
    completedCourses?: number;
    inProgressCourses?: number;
    totalTimeSpent?: number;
 * Action to retrieve users from LearnWorlds LMS
@RegisterClass(BaseAction, 'GetLearnWorldsUsersAction')
export class GetLearnWorldsUsersAction extends LearnWorldsBaseAction {
        return 'Retrieves users (students, instructors, admins) from LearnWorlds LMS with filtering and search options';
            const role = this.getParamValue(params.Params, 'Role');
            if (role) {
                queryParams.role = role;
            // Tag filter
            // Include course stats
            const includeCourseStats = this.getParamValue(params.Params, 'IncludeCourseStats');
            if (includeCourseStats) {
                queryParams.include = 'course_stats';
            queryParams.limit = Math.min(maxResults, 100); // LearnWorlds max is usually 100
            const users = await this.makeLearnWorldsPaginatedRequest<any>(
            const mappedUsers: LearnWorldsUser[] = users.map(user => this.mapLearnWorldsUser(user));
            const summary = this.calculateUserSummary(mappedUsers);
            if (!params.Params.find(p => p.Name === 'Users')) {
                    Name: 'Users',
                    Value: mappedUsers
                params.Params.find(p => p.Name === 'Users')!.Value = mappedUsers;
                    Value: mappedUsers.length
                params.Params.find(p => p.Name === 'TotalCount')!.Value = mappedUsers.length;
                Message: `Successfully retrieved ${mappedUsers.length} users from LearnWorlds`
     * Map LearnWorlds user data to our interface
    private mapLearnWorldsUser(lwUser: any): LearnWorldsUser {
            totalCourses: lwUser.course_stats?.total || 0,
            completedCourses: lwUser.course_stats?.completed || 0,
            inProgressCourses: lwUser.course_stats?.in_progress || 0,
            totalTimeSpent: lwUser.course_stats?.total_time_spent || 0,
            avatarUrl: lwUser.avatar_url,
            bio: lwUser.bio,
            location: lwUser.location,
            timezone: lwUser.timezone
     * Calculate summary statistics
    private calculateUserSummary(users: LearnWorldsUser[]): any {
            totalUsers: users.length,
            activeUsers: users.filter(u => u.status === 'active').length,
            inactiveUsers: users.filter(u => u.status === 'inactive').length,
            suspendedUsers: users.filter(u => u.status === 'suspended').length,
            usersByRole: {} as Record<string, number>,
            averageCoursesPerUser: 0,
            mostActiveUsers: [] as any[],
            recentSignups: [] as any[]
        // Count by role
        users.forEach(user => {
            summary.usersByRole[user.role] = (summary.usersByRole[user.role] || 0) + 1;
            summary.totalTimeSpent += user.totalTimeSpent || 0;
        if (users.length > 0) {
            const totalCourses = users.reduce((sum, u) => sum + (u.totalCourses || 0), 0);
            summary.averageCoursesPerUser = totalCourses / users.length;
        // Find most active users (by completed courses)
        summary.mostActiveUsers = users
            .filter(u => u.completedCourses && u.completedCourses > 0)
            .sort((a, b) => (b.completedCourses || 0) - (a.completedCourses || 0))
            .map(u => ({
                id: u.id,
                name: u.fullName || u.email,
                completedCourses: u.completedCourses
        // Find recent signups (last 30 days)
        const thirtyDaysAgo = new Date();
        thirtyDaysAgo.setDate(thirtyDaysAgo.getDate() - 30);
        summary.recentSignups = users
            .filter(u => u.createdAt > thirtyDaysAgo)
            .sort((a, b) => b.createdAt.getTime() - a.createdAt.getTime())
            .slice(0, 10)
                signupDate: u.createdAt
                Name: 'IncludeCourseStats',
