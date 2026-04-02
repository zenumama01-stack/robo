import { BaseEntity, PotentialDuplicateRequest } from "@memberjunction/core";
import { MJDuplicateRunEntity } from "@memberjunction/core-entities";
import { DuplicateRecordDetector } from "@memberjunction/ai-vector-dupe";
export class DuplicateRunEntity_Server extends MJDuplicateRunEntity  {
        if (saveResult && this.EndedAt === null) {
            // do something
            const duplicateRecordDetector: DuplicateRecordDetector = new DuplicateRecordDetector();
            let request: PotentialDuplicateRequest = new PotentialDuplicateRequest();
            request.EntityID = this.EntityID;
            request.ListID = this.SourceListID;
            request.Options = {
                DuplicateRunID: this.ID,
            const response = await duplicateRecordDetector.getDuplicateRecords(request, this.ContextCurrentUser);
