import { FilterBuilderComponent } from './filter-builder/filter-builder.component';
import { FilterGroupComponent } from './filter-group/filter-group.component';
import { FilterRuleComponent } from './filter-rule/filter-rule.component';
 * FilterBuilderModule
 * Provides a complete filter builder UI for creating complex
 * boolean filter expressions. Outputs Kendo-compatible
 * CompositeFilterDescriptor JSON format.
 * import { FilterBuilderModule } from '@memberjunction/ng-filter-builder';
 *   imports: [FilterBuilderModule],
 *   // ...
 * <mj-filter-builder
 *   [fields]="filterFields"
 *   [filter]="currentFilter"
 *   (filterChange)="onFilterChange($event)">
 * </mj-filter-builder>
    FilterBuilderComponent,
    FilterGroupComponent,
    FilterRuleComponent
export class FilterBuilderModule {}
