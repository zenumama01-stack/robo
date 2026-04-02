import { FormResponse } from '../../../base/base-form-builder.action';
 * Action to retrieve submissions from a JotForm with comprehensive filtering options
 *   ActionName: 'Get JotForm Submissions',
 *     Value: JSON.stringify({ status: 'ACTIVE', created_at: 'gt:2024-01-01' })
 *     Name: 'GetAllPages',
@RegisterClass(BaseAction, 'GetJotFormSubmissionsAction')
export class GetJotFormSubmissionsAction extends JotFormBaseAction {
        return 'Retrieves submissions from a JotForm with filtering, pagination, and ordering capabilities. Supports date ranges, status filtering, and custom field filters.';
            const limit = this.getParamValue(params.Params, 'Limit') || 100;
            const offset = this.getParamValue(params.Params, 'Offset') || 0;
            const orderby = this.getParamValue(params.Params, 'OrderBy');
            const getAllPages = this.getParamValue(params.Params, 'GetAllPages') === true;
            // Parse filter if provided (expects JSON string)
            let submissions: FormResponse[];
            let totalCount: number;
            let actualLimit: number;
            let actualOffset: number;
            if (getAllPages) {
                    orderby,
                submissions = jfSubmissions.map(s => this.normalizeJotFormSubmission(s));
                totalCount = submissions.length;
                actualLimit = submissions.length;
                actualOffset = 0;
                    limit: Math.min(limit, 1000),
                submissions = result.content.map(s => this.normalizeJotFormSubmission(s));
                totalCount = submissions.length; // JotForm doesn't return total count in API
                actualLimit = result.limit;
                actualOffset = result.offset;
                    Name: 'Submissions',
                    Value: submissions
                    Value: totalCount
                    Value: actualLimit
                    Name: 'Offset',
                    Value: actualOffset
                    Name: 'Count',
                Message: `Successfully retrieved ${submissions.length} submissions from JotForm${getAllPages ? '' : ` (offset: ${actualOffset}, limit: ${actualLimit})`}`
                Message: this.buildFormErrorMessage('Get JotForm Submissions', errorMessage, error)
                Value: 0
                Name: 'OrderBy',
                Name: 'GetAllPages',
