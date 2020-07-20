import * as Contract from '../contract/Contract';
import * as AuditTrailContract from '../contract/AuditTrailContract';
import * as Common from '../Common';

export class AuditTrailFormatter {
  private readonly tableStartElement = '<table class="table">';
  private readonly customFieldMap: AuditTrailContract.StringDictionary = {};
  private readonly customerFormatter: CustomerFormatter;
  private readonly departmentFormatter: DepartmentFormatter;
  private readonly userFormatter: UserFormatter;
  private readonly widgetAppearanceFormatter: WidgetAppearanceFormatter;
  private readonly widgetOverloadFormatter: WidgetOverloadFormatter;
  private readonly widgetUnknownDomainFormatter: WidgetUnknownDomainFormatter;
  private readonly widgetUnknownDomainTooManyFormatter: WidgetUnknownDomainTooManyFormatter;

  public constructor(public readonly auditTrailMaps: AuditTrailContract.AuditTrailMaps)
  {
    this.customerFormatter = new CustomerFormatter(this);
    this.departmentFormatter = new DepartmentFormatter(this);
    this.userFormatter = new UserFormatter(this);
    this.widgetAppearanceFormatter = new WidgetAppearanceFormatter();
    this.widgetOverloadFormatter = new WidgetOverloadFormatter();
    this.widgetUnknownDomainFormatter = new WidgetUnknownDomainFormatter();
    this.widgetUnknownDomainTooManyFormatter = new WidgetUnknownDomainTooManyFormatter();

    const m = this.customFieldMap;
    m['ExceptionMessage'] = 'Error';
    m['ClientIp'] = 'IP';
    m['ClientType'] = 'Client type';
    m['ClientVersion'] = 'Client version';
    m['ClientLocalDate'] = 'Client local date';
  }

  public format(history: AuditTrailContract.AuditEvent, record: AuditTrailContract.AuditRecord): void
  {
    record.Id = history.Id;
    record.Timestamp = history.Timestamp;
    record.Operation = this.auditTrailMaps.operationKindNames.get(history.Operation);
    record.Status = this.auditTrailMaps.operationStatusNames.get(history.Status);
    record.Author = this.getAuthorName(history.Author);
    record.Name = this.getObjectName(history);
    record.History = history;
  }

  private getAuthorName(author: Contract.Facet): string
  {
    if (null != author && null != author.Name && 0 < author.Name.length)
      return author.Name;

    return '';
  }

  public buildAuditDetails(history: AuditTrailContract.AuditEvent): string
  {
    console.log(`buildAuditDetails(${history.Id})`);

    let result = this.formatChanges(history);

    if (null != history.CustomValues)
    {
      const temp = this.customValuesFormat(history.CustomValues);
      result += temp;
    }

    return result;
  }

  private getCustomPairs(dictionary: AuditTrailContract.StringDictionary): Array<any>
  {
    const pairs: Array<any> = [];
    for (let key in this.customFieldMap)
    {
      if (this.customFieldMap.hasOwnProperty(key))
      {
        const value = dictionary[key];
        if (null != value && 0 < value.length)
        {
          const name = this.customFieldMap[key];
          const p = { k: name, v: value };
          pairs.push(p);
        }
      }
    }
    return pairs;
  }

  private customValuesFormat(dictionary: AuditTrailContract.StringDictionary): string
  {
    const pairs = this.getCustomPairs(dictionary);
    if (0 === pairs.length)
      return '';

    let result = this.tableStartElement
      + '<thead>'
      + '<tr><th>Parameter</th><th>Value</th></tr>'
      + '</thead>';

    for (const p of pairs)
    {
      const row = `<tr><td>${p.k}</td><td>${Common.escapeHtml(p.v)}</td></tr>`;
      result += row;
    }

    result += '</table>';
    return result;
  }

  private formatChanges(history: AuditTrailContract.AuditEvent): string
  {
    const hasOld = null != history.OldValue, hasNew = null != history.NewValue;
    const formatter = this.getFormatter(history.Operation) as AuditTrailContract.Formatter;
    if ((!hasOld && !hasNew) || null == formatter)
      return '';

    const body1 = formatter.format(history);
    if (null == body1 || 0 === body1.length)
      return '';

    let result = this.tableStartElement
      + '<thead><tr>'
      + '<th>Field</th>';

    if (hasOld)
      result += '<th>Old</th>';
    if (hasNew)
      result += '<th>New</th>';

    result += '</tr></thead>';

    result += body1;

    result += '</table>';
    return result;
  }

  private getFormatter(operation: string): AuditTrailContract.Formatter | null
  {
    switch (operation)
    {
    case 'UserChangePassword':
    case 'UserDelete':
    case 'UserInsert':
    case 'UserUpdate':
    case 'Login':
      return this.userFormatter;
    case 'DepartmentDelete':
    case 'DepartmentInsert':
    case 'DepartmentUpdate':
      return this.departmentFormatter;
    case 'CustomerInsert':
    case 'CustomerUpdate':
      return this.customerFormatter;
    case 'WidgetAppearanceUpdate':
      return this.widgetAppearanceFormatter;
    case 'WidgetOverload':
      return this.widgetOverloadFormatter;
    case 'WidgetUnknownDomain':
      return this.widgetUnknownDomainFormatter;
    case 'WidgetUnknownDomainTooManyEvent':
      return this.widgetUnknownDomainTooManyFormatter;
    default:
      console.error(`Formatter unknown operation=${operation}`);
      return null;
    }
  }

  private getObjectName(history: AuditTrailContract.AuditEvent): string
  {
    switch (history.Operation)
    {
    case 'UserChangePassword':
    case 'UserDelete':
    case 'UserInsert':
    case 'UserUpdate':
    case 'Login':
      {
        const result = this.getUserName(history.NewValue as Contract.UserInfo);
        if (0 < result.length)
          return result;
      }
      {
        const result = this.getUserName(history.OldValue as Contract.UserInfo);
        if (0 < result.length)
          return result;
      }
      break;

    //Department
    case 'DepartmentDelete':
    case 'DepartmentInsert':
    case 'DepartmentUpdate':
    //Customer
    case 'CustomerInsert':
    case 'CustomerUpdate':
      {
        const result = this.getName(history.NewValue as Contract.INamed);
        if (0 < result.length)
          return result;
      }
      {
        const result = this.getName(history.OldValue as Contract.INamed);
        if (0 < result.length)
          return result;
      }
      break;
    case 'WidgetUnknownDomain':
      if (null != history.NewValue)
      {
        const entity = history.NewValue as Contract.WidgetUnknownDomain;
        if (null != entity)
        {
          const result = this.getName(entity as Contract.INamed);
          return result;
        }
      }
      break;
    case 'WidgetOverload':
    case 'WidgetUnknownDomainTooManyEvent':
      if (null != history.NewValue)
      {
        const entity = history.NewValue as Contract.ILimit;
        if (null != entity && null != entity.Limit)
        {
          const result = entity.Limit.toString();
          return result;
        }
      }
      break;
    case 'WidgetAppearanceUpdate':
      break;
    default:
      console.error(`Object name unknown operation=${history.Operation}`);
      break;
    }

    return '';
  }

  private getName(value: Contract.INamed): string
  {
    if (null != value && null != value.Name && 0 < value.Name.length)
      return value.Name;
    return '';
  }

  private getUserName(value: Contract.UserInfo): string
  {
    if (null != value && (null != value.FirstName && 0 < value.FirstName.length))
    {
      if (null != value.LastName && 0 < value.LastName.length)
      {
        const result = value.FirstName + ' ' + value.LastName;
        return result;
      }
      return value.FirstName;
    }
    return '';
  }
}


class FormatFuncs {
  private static row1(displayName: string, value: string, skipEscape?: boolean): string
  {
    if (null == value || 0 === value.length)
      return '';

    const val2 = !!skipEscape ? value : Common.escapeHtml(value);
    const result = `<tr><td>${displayName}</td><td>${val2}</td></tr>`;
    return result;
  }

  private static row2(displayName: string,
    oldValue: string,
    newValue: string,
    skipEscape?: boolean,
    skipClasses?: boolean): string
  {
    const hasOld = null != oldValue && 0 < oldValue.length,
      hasNew = null != newValue && 0 < newValue.length;
    if (!hasOld && !hasNew)
      return '';

    const formOld = hasOld
                      ? (!!skipEscape ? oldValue : Common.escapeHtml(oldValue))
                      : '',
      formNew = hasNew
                  ? (!!skipEscape ? newValue : Common.escapeHtml(newValue))
                  : '';

    const areSame = formOld === formNew;
    if (areSame)
    {
      const result = `<tr><td>${displayName}</td><td>${formOld}</td><td>${formNew}</td></tr>`;
      return result;
    }
    {
      const deletedClass = !skipClasses ? 'class="deleted"' : '';
      const insertedClass = !skipClasses ? 'class="inserted"' : '';
      const changedClass = !skipClasses ? 'class="changed-row"' : '';
      const result =
        `<tr ${changedClass}><td>${displayName}</td><td ${deletedClass}>${formOld}</td><td ${insertedClass}>${formNew
          }</td></tr>`;
      return result;
    }
  }

  public static formatForString<T>(fieldName: string, oldInstance: T, newInstance: T, displayName?: string): string
  {
    const hasOld = null != oldInstance, hasNew = null != newInstance;

    const oldName = hasOld ? (oldInstance as any)[fieldName] : null,
      newName = hasNew ? (newInstance as any)[fieldName] : null;

    const result = this.formatStrings(hasOld, hasNew, fieldName, displayName as string, oldName, newName);
    return result;
  }

  public static formatForBool<T>(fieldName: string, oldInstance: T, newInstance: T, displayName?: string): string
  {
    const hasOld = null != oldInstance, hasNew = null != newInstance;

    const oldBool = hasOld ? (oldInstance as any)[fieldName] : null,
      newBool = hasNew ? (newInstance as any)[fieldName] : null;

    const oldName = !!oldBool ? 'Yes' : 'No', newName = !!newBool ? 'Yes' : 'No';
    const result = this.formatStrings(hasOld, hasNew, fieldName, displayName as string, oldName, newName, true);
    return result;
  }

  public static formatForNumber<T>(
    fieldName: string,
    oldInstance: T,
    newInstance: T,
    displayName?: string): string
  {
    const hasOld = null != oldInstance, hasNew = null != newInstance;

    const oldName = FormatFuncs.numberToString(oldInstance, fieldName),
      newName = FormatFuncs.numberToString(newInstance, fieldName);

    const result = this.formatStrings(hasOld, hasNew, fieldName, displayName as string, oldName, newName);
    return result;
  }

  private static numberToString<T>(instance: T, fieldName: string):
    string
  {
    if (null != instance)
    {
      const id = (instance as any)[fieldName];
      if ('number' === typeof (id))
      {
        const name = id.toString();
        return name;
      }
    }
    return '';
  }

  public static formatForEnum<T>(
    fieldName: string,
    oldInstance: T,
    newInstance: T,
    idToNameMapper: Contract.IdToNameMap,
    displayName?: string): string
  {
    const hasOld = null != oldInstance, hasNew = null != newInstance;

    const oldName = FormatFuncs.enumToString(oldInstance, fieldName, idToNameMapper),
      newName = FormatFuncs.enumToString(newInstance, fieldName, idToNameMapper);

    const result = this.formatStrings(hasOld, hasNew, fieldName, displayName as string, oldName, newName);
    return result;
  }

  private static enumToString<T>(instance: T, fieldName: string, idToNameMapper: Contract.IdToNameMap):
    string
  {
    if (null != instance)
    {
      const raw = (instance as any)[fieldName];
      let id: string | null = null;
      if ('number' === typeof (raw))
        id = '' + raw;
      else if ('string' === typeof (raw))
        id = raw;

      if ('string' === typeof (id))
      {
        const name = idToNameMapper.get(id);
        if (null != name && 0 < name.length)
          return name;
      }
    }
    return '';
  }

  private static formatStrings(hasOld: boolean,
    hasNew: boolean,
    fieldName: string,
    displayName: string,
    oldValue: string,
    newValue: string,
    skipEscape?: boolean): string
  {
    if (null == displayName || 0 === displayName.length)
      displayName = fieldName;

    if (hasOld)
    {
      if (hasNew)
      {
        const result = this.row2(displayName, oldValue, newValue, skipEscape);
        return result;
      }
      {
        const result = this.row1(displayName, oldValue, skipEscape);
        return result;
      }
    }
    if (hasNew)
    {
      const result = this.row1(displayName, newValue, skipEscape);
      return result;
    }
    return '';
  }

  public static customObjectToDictionary(
    history: AuditTrailContract.AuditEvent,
    objectName: string): AuditTrailContract.StringDictionary | null
  {
    const arr =
      (null == history.CustomObjects ? null : history.CustomObjects[objectName]) as Array<Contract.IdName<string>>;
    if (null == arr || 0 === arr.length)
      return null;

    const result: any = {};
    for (const p of arr)
    {
      result[p.Id] = p.Name;
    }

    return result;
  }

  public static formatForIdList<T>(
    idToNameMap: AuditTrailContract.StringDictionary,
    fieldName: string,
    oldInstance: T,
    newInstance: T,
    objectName: string,
    displayName?: string): string
  {
    if (null == idToNameMap)
      return '';

    if (null == displayName)
      displayName = fieldName;

    const hasOld = null != oldInstance, hasNew = null != newInstance;
    if (hasOld)
    {
      if (hasNew)
      {
        const result1 = this.formatForIdListTwo(idToNameMap, fieldName, oldInstance, newInstance, displayName);
        return result1;
      }

      const result = this.formatForIdListOne(idToNameMap, fieldName, oldInstance, displayName);
      return result;
    }
    if (hasNew)
    {
      const result = this.formatForIdListOne(idToNameMap, fieldName, newInstance, displayName);
      return result;
    }
    return '';
  }

  private static formatForIdListOne<T>(
    idToNameMap: AuditTrailContract.StringDictionary,
    fieldName: string,
    instance: T,
    displayName: string): string
  {
    const arr1 = ((instance as any)[fieldName]) as Array<number>;
    if (null == arr1 || 0 === arr1.length)
      return '';

    const names = this.arrayMapToString(arr1, idToNameMap);
    const result = this.row1(displayName, names, true);
    return result;
  }

  //Numbers in JS are sorted ... as if numbers were strings.
  private static sortNumbers(arr: Array<number>)
  {
    if (null == arr || 0 === arr.length)
      return;

    arr.sort((a, b) => a - b);
  }

  //Each array must have at least one element.
  //Arrays will be sorted.
  private static numberArrayDifference(arr1: Array<number>, arr2: Array<number>)
  {
    FormatFuncs.sortNumbers(arr1);
    FormatFuncs.sortNumbers(arr2);

    const result = this.arrayDifference(arr1, arr2);
    return result;
  }

  //Each array must have at least one element.
  //Arrays will be sorted.
  private static stringArrayDifference(arr1: Array<string>, arr2: Array<string>)
  {
    arr1.sort();
    arr2.sort();

    const result = this.arrayDifference(arr1, arr2);
    return result;
  }

  //Arrays must be sorted and have at least one element.
  private static arrayDifference<T>(arr1: Array<T>, arr2: Array<T>)
  {
    const stayed: Array<T> = [], deleted: Array<T> = [], inserted: Array<T> = [];

    const len1 = arr1.length, len2 = arr2.length;
    let i1 = 0, i2 = 0;

    while (i1 < len1 && i2 < len2)
    {
      const v1 = arr1[i1], v2 = arr2[i2];
      if (v1 === v2)
      {
        stayed.push(v1);
        ++i1;
        ++i2;
      }
      else if (v1 < v2)
      {
        deleted.push(v1);
        ++i1;
      }
      else
      {
        inserted.push(v2);
        ++i2;
      }
    }

    while (i1 < len1)
    {
      const v1 = arr1[i1];
      deleted.push(v1);
      ++i1;
    }

    while (i2 < len2)
    {
      const v2 = arr2[i2];
      inserted.push(v2);
      ++i2;
    }

    const result = {
        'deleted': deleted,
        'inserted': inserted,
        'stayed': stayed
      };
    return result;
  }

  //The result doesn't have to be escaped.
  private static arrayMapToString<T>(values: Array<T>, idToNameMap: any): string
  {
    const list: Array<string> = [];
    for (const id of values)
    {
      const name = idToNameMap[id];
      if (null == name || 0 === name.length)
        continue;
      list.push(Common.escapeHtml(name));
    }

    list.sort();

    let result = '';
    for (const name of list)
    {
      if (0 < result.length)
        result += ', ';
      result += name;
    }

    return result;
  }

  //The "values" will be sorted.
  //The result doesn't have to be escaped.
  private static arrayToString(values: Array<string>): string
  {
    if (null == values || 0 === values.length)
      return '';

    values.sort();

    let result = '';
    for (const name of values)
    {
      if (0 < result.length)
        result += ', ';
      result += name;
    }

    return result;
  }

  private static spanInClass(value: string, className: string)
  {
    const result = `<span class="${className}">${value}</span>`;
    return result;
  }

  private static formatForIdListTwo<T>(
    idToNameMap: AuditTrailContract.StringDictionary,
    fieldName: string,
    oldInstance: T,
    newInstance: T,
    displayName: string): string
  {
    const oldValue = ((oldInstance as any)[fieldName]) as Array<number>,
      newValue = ((newInstance as any)[fieldName]) as Array<number>;

    const hasOld = null != oldValue && 0 < oldValue.length,
      hasNew = null != newValue && 0 < newValue.length;

    let older = '', newer = '';
    if (hasOld)
    {
      if (hasNew)
      {
        const diff = this.numberArrayDifference(oldValue, newValue);

        const stayed = this.arrayMapToString(diff.stayed, idToNameMap),
          deleted = this.arrayMapToString(diff.deleted, idToNameMap),
          inserted = this.arrayMapToString(diff.inserted, idToNameMap);

        const oldNew = this.getOlderNewer(stayed, deleted, inserted);
        older = oldNew.older;
        newer = oldNew.newer;
      }
      else
      { //Old only
        const temp = this.arrayMapToString(oldValue, idToNameMap);
        if (null != temp && 0 < temp.length)
          older = this.spanInClass(temp, AuditTrailContract.className.deleted);
      }
    }
    else if (hasNew)
    {
      const temp = this.arrayMapToString(newValue, idToNameMap);
      if (null != temp && 0 < temp.length)
        newer = this.spanInClass(temp, AuditTrailContract.className.inserted);
    }

    const result = this.row2(displayName, older, newer, true, true);
    return result;
  }

  public static formatForStringArray<T>(
    fieldName: string,
    oldInstance: T,
    newInstance: T,
    displayName?: string): string
  {
    if (null == displayName)
      displayName = fieldName;

    const hasOld = null != oldInstance, hasNew = null != newInstance;
    if (hasOld)
    {
      if (hasNew)
      {
        const result1 = this.formatForStringArrayTwo(fieldName, oldInstance, newInstance, displayName);
        return result1;
      }

      const result = this.formatForStringArrayOne(fieldName, oldInstance, displayName);
      return result;
    }
    if (hasNew)
    {
      const result = this.formatForStringArrayOne(fieldName, newInstance, displayName);
      return result;
    }
    return '';
  }

  private static formatForStringArrayOne<T>(
    fieldName: string,
    instance: T,
    displayName: string): string
  {
    const arr1 = ((instance as any)[fieldName]) as Array<string>;
    if (null == arr1 || 0 === arr1.length)
      return '';

    const names = this.arrayToString(arr1);
    const result = this.row1(displayName, names, true);
    return result;
  }

  private static formatForStringArrayTwo<T>(
    fieldName: string,
    oldInstance: T,
    newInstance: T,
    displayName: string): string
  {
    const oldValue = ((oldInstance as any)[fieldName]) as Array<string>,
      newValue = ((newInstance as any)[fieldName]) as Array<string>;

    const hasOld = null != oldValue && 0 < oldValue.length,
      hasNew = null != newValue && 0 < newValue.length;

    let older = '', newer = '';
    if (hasOld)
    {
      if (hasNew)
      {
        const diff = this.stringArrayDifference(oldValue, newValue);

        const stayed = this.arrayToString(diff.stayed),
          deleted = this.arrayToString(diff.deleted),
          inserted = this.arrayToString(diff.inserted);

        const oldNew = this.getOlderNewer(stayed, deleted, inserted);
        older = oldNew.older;
        newer = oldNew.newer;
      }
      else
      { //Old only
        const temp = this.arrayToString(oldValue);
        if (null != temp && 0 < temp.length)
          older = this.spanInClass(temp, AuditTrailContract.className.deleted);
      }
    }
    else if (hasNew)
    {
      const temp = this.arrayToString(newValue);
      if (null != temp && 0 < temp.length)
        newer = this.spanInClass(temp, AuditTrailContract.className.inserted);
    }

    const result = this.row2(displayName, older, newer, true, true);
    return result;
  }

  private static getOlderNewer(stayed: string, deleted: string, inserted: string)
  {
    let older = '', newer = '';
    if (0 < stayed.length && 0 < deleted.length)
      older = stayed + ', ' + this.spanInClass(deleted, AuditTrailContract.className.deleted);
    else if (0 < stayed.length)
      older = stayed;
    else if (0 < deleted.length)
      older = this.spanInClass(deleted, AuditTrailContract.className.deleted);

    if (0 < stayed.length && 0 < inserted.length)
      newer = stayed + ', ' + this.spanInClass(inserted, AuditTrailContract.className.inserted);
    else if (0 < stayed.length)
      newer = stayed;
    else if (0 < inserted.length)
      newer = this.spanInClass(inserted, AuditTrailContract.className.inserted);

    return { 'older': older, 'newer': newer };
  }
}

class CustomerFormatter implements AuditTrailContract.Formatter {
  constructor(private readonly formatter: AuditTrailFormatter)
  {}

  public format(history: AuditTrailContract.AuditEvent): string
  {
    const oldValue = history.OldValue as Contract.CustomerInfo,
      newValue = history.NewValue as Contract.CustomerInfo;

    let result = '';
    {
      const row = FormatFuncs.formatForEnum(
        'Status',
        oldValue,
        newValue,
        this.formatter.auditTrailMaps.objectStatusNames);
      result += row;
    }
    {
      const row = FormatFuncs.formatForString('Name', oldValue, newValue);
      result += row;
    }
    {
      const row = FormatFuncs.formatForString('CreateIp', oldValue, newValue);
      result += row;
    }
    {
      const row = FormatFuncs.formatForStringArray('Domains', oldValue, newValue);
      result += row;
    }
    return result;
  }
}

class DepartmentFormatter implements AuditTrailContract.Formatter {
  constructor(private readonly formatter: AuditTrailFormatter)
  {}

  public format(history: AuditTrailContract.AuditEvent): string
  {
    const oldValue = history.OldValue as Contract.DepartmentInfo,
      newValue = history.NewValue as Contract.DepartmentInfo;

    let result = '';
    {
      const row = FormatFuncs.formatForEnum(
        'Status',
        oldValue,
        newValue,
        this.formatter.auditTrailMaps.objectStatusNames);
      result += row;
    }
    {
      const row = FormatFuncs.formatForBool('IsPublic', oldValue, newValue);
      result += row;
    }
    {
      const row = FormatFuncs.formatForString('Name', oldValue, newValue);
      result += row;
    }
    {
      const row = FormatFuncs.formatForString('Description', oldValue, newValue);
      result += row;
    }
    return result;
  }
}

class WidgetAppearanceFormatter implements AuditTrailContract.Formatter {
  public format(history: AuditTrailContract.AuditEvent): string
  {
    const oldValue = history.OldValue as Contract.ChatWidgetAppearance,
      newValue = history.NewValue as Contract.ChatWidgetAppearance;

    let result = '';
    {
      const row = FormatFuncs.formatForString('customCssUrl', oldValue, newValue, 'Background Page');
      result += row;
    }
    {
      const row = FormatFuncs.formatForString('themeId', oldValue, newValue, 'Theme');
      result += row;
    }
    {
      const row = FormatFuncs.formatForString('themeMinId', oldValue, newValue, 'Theme minimized');
      result += row;
    }
    {
      const row = FormatFuncs.formatForString('location', oldValue, newValue, 'Location');
      result += row;
    }
    {
      const row = FormatFuncs.formatForNumber('offsetX', oldValue, newValue, 'Offset X');
      result += row;
    }
    {
      const row = FormatFuncs.formatForNumber('offsetY', oldValue, newValue, 'Offset Y');
      result += row;
    }
    {
      const row = FormatFuncs.formatForString('minStateTitle', oldValue, newValue, 'Minimized State Title');
      result += row;
    }
    {
      const row = FormatFuncs.formatForBool('poweredByVisible', oldValue, newValue, 'Hide "Powered By" message');
      result += row;
    }

    return result;
  }
}

class WidgetOverloadFormatter implements AuditTrailContract.Formatter {
  public format(history: AuditTrailContract.AuditEvent): string
  {
    const newValue = history.NewValue as Contract.WidgetDailyViewCountExceededEvent;
    if (null == newValue)
      return '';

    const message = `You have exceeded the allowed limit (${newValue.Limit}) of Widget loads per day.`;
    const result = `<tr><td>Message</td><td>${message}</td></tr>`;
    return result;
  }
}

class WidgetUnknownDomainFormatter implements AuditTrailContract.Formatter {
  public format(history: AuditTrailContract.AuditEvent): string
  {
    const newValue = history.NewValue as Contract.WidgetUnknownDomain;
    if (null == newValue)
      return '';

    const message =
      `An attempt to show the Chat Widget on a page loaded from the unregistered domain '${newValue.Name
        }'. Your current domain list is '${newValue.Domains}'.`;

    const result = `<tr><td>Message</td><td>${message}</td></tr>`;
    return result;
  }
}

class WidgetUnknownDomainTooManyFormatter implements AuditTrailContract.Formatter {
  public format(history: AuditTrailContract.AuditEvent): string
  {
    const newValue = history.NewValue as Contract.WidgetUnknownDomainTooManyEvent;
    if (null == newValue)
      return '';

    const message =
      `Too many Chat Widget load attempts from unregistered domain names. Your current domain list is '${newValue
        .Domains}'.`;
    const result = `<tr><td>Message</td><td>${message}</td></tr>`;
    return result;
  }
}

class UserFormatter implements AuditTrailContract.Formatter {
  constructor(private readonly formatter: AuditTrailFormatter)
  {}

  public format(history: AuditTrailContract.AuditEvent): string
  {
    const oldValue = history.OldValue as Contract.UserInfo,
      newValue = history.NewValue as Contract.UserInfo;

    let result = '';
    {
      const row = FormatFuncs.formatForEnum(
        'Status',
        oldValue,
        newValue,
        this.formatter.auditTrailMaps.objectStatusNames);
      result += row;
    }
    {
      const row = FormatFuncs.formatForString('Email', oldValue, newValue);
      result += row;
    }
    {
      const row = FormatFuncs.formatForString('FirstName', oldValue, newValue, 'First name');
      result += row;
    }
    {
      const row = FormatFuncs.formatForString('LastName', oldValue, newValue, 'Last name');
      result += row;
    }
    {
      const row = FormatFuncs.formatForString('Avatar', oldValue, newValue);
      result += row;
    }
    {
      const row = FormatFuncs.formatForBool('IsOwner', oldValue, newValue, 'Is Customer Owner');
      result += row;
    }
    {
      const row = FormatFuncs.formatForBool('IsAdmin', oldValue, newValue, 'Is Administrator');
      result += row;
    }
    {
      const key = 'department';
      const departments = FormatFuncs.customObjectToDictionary(history, key) as AuditTrailContract.StringDictionary;
      if (null != departments)
      {
        {
          const row = FormatFuncs.formatForIdList(
            departments,
            'AgentDepartments',
            oldValue,
            newValue,
            'department',
            'Agent For');
          result += row;
        }
        {
          const row = FormatFuncs.formatForIdList(
            departments,
            'SupervisorDepartments',
            oldValue,
            newValue,
            'department',
            'Supervisor For');
          result += row;
        }
      }
      return result;
    }
  }
}