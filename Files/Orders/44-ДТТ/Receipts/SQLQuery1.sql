use AISExam_2018


if not object_id('tempdb..#Schools') is null drop table #Schools
Create table #Schools (
	SchoolSpecCode	int,
	SchoolSpecName nvarchar(200),
	GovernmentCode	int, 
	GovernmentName	nvarchar(200), 
	SchoolCode	int,
	ShortName 	nvarchar(250),
	SchoolTypeCode	int, 
	SchoolTypeName	nvarchar(200), 
	SchoolKindCode	int, 
	SchoolKindName	nvarchar(200),
	SchoolPropertyCode	int,
	SchoolPropertyName	nvarchar(200),
	SchoolName 	nvarchar(1000),
	SchoolID	uniqueidentifier,
	STUD		int default 0,
	REG			int default 0,
	OnSeat		int default 0,
	VTG			int default 0,
	SPO			int default 0,
	VPL			int default 0,
	SPR			int default 0,
	INOU			int default 0,
	KL10			int default 0
	)
create nonclustered index TMP_IxG on #Schools(GovernmentName)

insert #Schools
exec bvl_Schools


declare @subj int = 18
declare @max int
set @max = (select MAX(tr.B5max) from tblRates5 tr join Discipline d on d.ObjectID = tr.Discipline where tr.Class = 9 and d.Code = @subj)

select sch.GovernmentCode 'Код', sch.GovernmentName 'ОУО', COUNT(1) 'ОУ', t.RES 'Участников', t.av 'Ср.балл', t.m2 'Ниже порога', t.m2p '%', t.m100 'Макс. баллов'--, t.su
 from #Schools sch
join (
	select 
		sch.GovernmentCode, 
		COUNT(1) RES,
		avg(cast(fr.TestRate as float)) av,
		sum(fr.TestRate) su,
		SUM(case when fr.Rate5=2 then 1 else 0 end) m2,
		SUM(case when fr.Rate5=2 then 1 else 0 end) /cast(count(1) as float) m2p,
		SUM(case when fr.TestRate=@max then 1 else 0 end) m100
	from tblFinalRes9 fr 
	join #Schools sch on sch.SchoolID = fr.OU
	where ExamClass=9 and State=1 and SubjectCode = @subj
	--SubjectCode IN (1,2)

	group by sch.GovernmentCode
) t on t.GovernmentCode = sch.GovernmentCode
group by sch.GovernmentCode, sch.GovernmentName, t.av, t.m2, t.m2p, t.RES, t.m100--, t.su
union
select 1000, 'ВСЕГО:', COUNT(1) 'ОУ', t.RES 'Участников', t.av 'Ср.балл', t.m2 'Ниже порога', t.m2p '%', t.m100 'Макс. баллов'--, t.su
from #schools sch 
join (
	select 
		--sch.GovernmentCode, 
		COUNT(1) RES,
		avg(cast(fr.TestRate as float)) av,
		sum(fr.TestRate) su,
		SUM(case when fr.Rate5=2 then 1 else 0 end) m2,
		SUM(case when fr.Rate5=2 then 1 else 0 end) /cast(count(1) as float) m2p,
		SUM(case when fr.TestRate=@max then 1 else 0 end) m100
	from tblFinalRes9 fr 
	join #Schools sch on sch.SchoolID = fr.OU
	where ExamClass=9 and State=1 and SubjectCode = @subj
	--SubjectCode IN (1,2)
	--group by sch.GovernmentCode
) t on 1=1
group by t.av, t.m2, t.m2p, t.RES, t.m100--, t.su
order by 1

--select * 
--from tblFinalRes9 fr 
----join #Schools sch on sch.SchoolID = fr.OU
--where 
--ExamClass=9 and SubjectCode = @subj and State=1
--and sch.SchoolCode<2000 and fr.Rate5=2

--select MAX(tr.B5max) from tblRates5 tr
--join Discipline d on d.ObjectID = tr.Discipline
--where tr.Class = 9 and d.Code = @subj