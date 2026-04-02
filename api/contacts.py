class DeleteManualContactsArg(bb.Struct):
    :ivar contacts.DeleteManualContactsArg.email_addresses: List of manually
        added contacts to be deleted.
        '_email_addresses_value',
                 email_addresses=None):
        self._email_addresses_value = bb.NOT_SET
        if email_addresses is not None:
            self.email_addresses = email_addresses
    # Instance attribute type: list of [str] (validator is set below)
    email_addresses = bb.Attribute("email_addresses")
        super(DeleteManualContactsArg, self)._process_custom_annotations(annotation_type, field_path, processor)
DeleteManualContactsArg_validator = bv.Struct(DeleteManualContactsArg)
class DeleteManualContactsError(bb.Union):
    :ivar list of [str] contacts.DeleteManualContactsError.contacts_not_found:
        Can't delete contacts from this list. Make sure the list only has
        manually added contacts. The deletion was cancelled.
    def contacts_not_found(cls, val):
        Create an instance of this class set to the ``contacts_not_found`` tag
        :param list of [str] val:
        :rtype: DeleteManualContactsError
        return cls('contacts_not_found', val)
    def is_contacts_not_found(self):
        Check if the union tag is ``contacts_not_found``.
        return self._tag == 'contacts_not_found'
    def get_contacts_not_found(self):
        Only call this if :meth:`is_contacts_not_found` is true.
        :rtype: list of [str]
        if not self.is_contacts_not_found():
            raise AttributeError("tag 'contacts_not_found' not set")
        super(DeleteManualContactsError, self)._process_custom_annotations(annotation_type, field_path, processor)
DeleteManualContactsError_validator = bv.Union(DeleteManualContactsError)
DeleteManualContactsArg.email_addresses.validator = bv.List(common.EmailAddress_validator)
DeleteManualContactsArg._all_field_names_ = set(['email_addresses'])
DeleteManualContactsArg._all_fields_ = [('email_addresses', DeleteManualContactsArg.email_addresses.validator)]
DeleteManualContactsError._contacts_not_found_validator = bv.List(common.EmailAddress_validator)
DeleteManualContactsError._other_validator = bv.Void()
DeleteManualContactsError._tagmap = {
    'contacts_not_found': DeleteManualContactsError._contacts_not_found_validator,
    'other': DeleteManualContactsError._other_validator,
DeleteManualContactsError.other = DeleteManualContactsError('other')
delete_manual_contacts = bb.Route(
    'delete_manual_contacts',
delete_manual_contacts_batch = bb.Route(
    'delete_manual_contacts_batch',
    DeleteManualContactsArg_validator,
    DeleteManualContactsError_validator,
    'delete_manual_contacts': delete_manual_contacts,
    'delete_manual_contacts_batch': delete_manual_contacts_batch,
