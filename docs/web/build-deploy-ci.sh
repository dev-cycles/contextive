BUCKET_NAME=($(aws ssm get-parameter --name "/$CONTEXTIVE_STAGE/docs/bucketName" --query Parameter.Value --output text))
SHA=($(git rev-parse --short HEAD))
VERSION=($(jq .version package.json -r))
if [[ "$CONTEXTIVE_STAGE" = "test" ]]; then
    VERSION=$VERSION+$SHA
fi
export BASE_URL=/ide/$VERSION
export VERSION=$VERSION
npm run build
S3_URL=s3://$BUCKET_NAME$BASE_URL
echo Deploying to $S3_URL
aws s3 sync dist $S3_URL

if [[ -z "$CI" ]]; then

    GITHUB_JOB=`date +%s`
fi

DISTRIBUTION_ID=($(aws ssm get-parameter --name "/$CONTEXTIVE_STAGE/docs/distributionId" --query Parameter.Value --output text))
aws cloudfront create-invalidation \
    --distribution-id $DISTRIBUTION_ID \
    --invalidation-batch "Paths={Quantity=1,Items=[$BASE_URL/*]},CallerReference=$GITHUB_JOB"

if [[ "$CONTEXTIVE_STAGE" = "test" ]]; then
    echo Site live for preview at https://docs.test.contextive.tech$BASE_URL
else
    echo Site live at https://docs.contextive.tech$BASE_URL
fi